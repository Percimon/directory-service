using System.Globalization;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.Get;

public sealed record GetLocationsQuery(
    int Page,
    int PageSize,
    string SortBy,
    string SortDirection,
    int MinDepartmentsCount,
    string? Search) : IQuery;

public sealed class GetLocationsQueryValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithError(
                Error.Validation("location.get", "Page must be greater than 0."));

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithError(
                Error.Validation("location.get", "Page must be greater than 0."))
            .LessThanOrEqualTo(100).WithError(
                Error.Validation("location.get", "PageSize must be at most 100."));

        RuleFor(x => x.SortBy)
            .Custom((sortBy, context) =>
            {
                var validSortByFields = new[] { "name", "createdAt" };
                if (!validSortByFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                {
                    context.AddFailure(
                        JsonSerializer.Serialize(
                             Error.Validation(
                                "location.get",
                                $"SortBy must be one of the following: {string.Join(", ", validSortByFields)}.")));
                }
            });

        RuleFor(x => x.SortDirection)
            .NotEmpty().WithMessage("SortDirection is required.")
            .Must(direction => direction.Equals("asc", StringComparison.OrdinalIgnoreCase) || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithError(Error.Validation(
                "location.get",
                "SortDirection must be either 'asc' or 'desc'."));

        RuleFor(x => x.Search)
            .Must(x => string.IsNullOrWhiteSpace(x) ? true : x.Length <= Constants.TextLength.LENGTH_50)
            .WithError(Error.Validation(
                "location.get",
                "Search must be empty or less than 50 characters"));

        RuleFor(x => x.MinDepartmentsCount)
            .GreaterThanOrEqualTo(0).WithError(
                Error.Validation("location.get", "MinDepartmentsCount must be greater than or equal to 0."));
    }
}

public sealed class GetLocationsHandler : IQueryHandler<PagedList<LocationListItemDto>, GetLocationsQuery>
{
    private readonly IValidator<GetLocationsQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetLocationsHandler> _logger;

    public GetLocationsHandler(
        IValidator<GetLocationsQuery> validator,
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetLocationsHandler> logger)
    {
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<PagedList<LocationListItemDto>, Error>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var builder = new SqlBuilder();

        // Шаблон 1: Только для подсчета общего количества (БЕЗ пагинации и сортировки)
        var countSelector = builder.AddTemplate(
            $"""
            WITH filtered_locations AS (
                SELECT l.id
                FROM locations AS l
                LEFT JOIN department_locations AS d ON l.id = d.location_id
                /**where**/
                /**groupby**/
                /**having**/
            )
            SELECT COUNT(*) FROM filtered_locations;
            """);

        // Шаблон 2: Только для получения данных текущей страницы
        var dataSelector = builder.AddTemplate(
            $"""
            WITH filtered_locations AS (
                SELECT 
                    l.id, l.name, l.city, l.district, l.street, l.structure, l.created_at,
                    COUNT(d.department_id) AS DepartmentsCount
                FROM locations AS l
                LEFT JOIN department_locations AS d ON l.id = d.location_id
                /**where**/
                /**groupby**/
                /**having**/
            )
            SELECT * FROM filtered_locations AS fl
            /**orderby**/
            LIMIT @PageSize OFFSET @Offset;
            """);

        // --- Наполнение билдера (одинаково для обоих шаблонов) ---
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            builder.Where("l.name ILIKE @Search", new { Search = $"%{query.Search}%" });
        }

        builder.GroupBy("l.id, l.name, l.city, l.district, l.street, l.structure, l.created_at");

        if (query.MinDepartmentsCount > 0)
        {
            builder.Having("COUNT(d.department_id) >= @MinDepartmentsCount", new { query.MinDepartmentsCount });
        }

        // Настройки сортировки и пагинации (повлияют только на dataSelector, так как в countSelector нет этих макросов)
        string columnOrder = query.SortBy?.ToLower(CultureInfo.CurrentCulture) switch
        {
            "name" => "fl.name",
            "createdat" => "fl.created_at",
            _ => "fl.name"
        };

        string directionOrder = query.SortDirection?.ToLower(CultureInfo.CurrentCulture) == "desc" ? "DESC" : "ASC";

        builder.OrderBy($"{columnOrder} {directionOrder}");

        int offset = (query.Page - 1) * query.PageSize;

        builder.AddParameters(new { query.PageSize, Offset = offset });

        // --- Выполнение в БД ---
        try
        {
            using var connection = _sqlConnectionFactory.Create();

            // 1. Сначала всегда получаем точный TotalCount (он вернет число, даже если выборка пустая)
            long totalCount = await connection.QueryFirstOrDefaultAsync<long>(countSelector.RawSql, countSelector.Parameters);

            List<LocationListItemDto> items = [];

            // 2. Делаем запрос за данными, только если общее количество больше нуля
            if (totalCount > 0)
            {
                items = (await connection.QueryAsync<LocationListItemDto>(
                    dataSelector.RawSql,
                    dataSelector.Parameters)).ToList();
            }

            return new PagedList<LocationListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving locations.");
            return Error.Failure("location.get", "An error occurred while retrieving locations.");
        }

    }
}