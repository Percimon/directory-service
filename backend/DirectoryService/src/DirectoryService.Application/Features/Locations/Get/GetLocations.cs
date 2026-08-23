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

        var selector = builder.AddTemplate(
        $"""
        WITH filtered_locations AS (
            SELECT 
                l.id, 
                l.name,
                l.city, l.district, l.street, l.structure,
                l.created_at,
                COUNT(d.department_id) AS DepartmentsCount,
                COUNT(*) OVER() AS TotalCount
            FROM locations AS l
            LEFT JOIN department_locations AS d ON l.id = d.location_id
            /**where**/
            /**groupby**/
            /**having**/
        )
        SELECT 
            fl.id, fl.name, fl.city, fl.district, fl.street, fl.structure,
            fl.created_at, fl.DepartmentsCount, fl.TotalCount
        FROM filtered_locations AS fl
        /**orderby**/
        LIMIT @PageSize OFFSET @Offset;
        """);

        // 3. Динамически добавляем условия WHERE, GROUP BY и HAVING
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Используем ILIKE для PostgreSQL (или LOWER(l.name) LIKE LOWER(@Search) для других СУБД)
            builder.Where("l.name ILIKE @Search", new { Search = $"%{query.Search}%" });
        }

        builder.GroupBy("l.id, l.name, l.city, l.district, l.street, l.structure, l.created_at");

        if (query.MinDepartmentsCount > 0)
        {
            builder.Having("COUNT(d.department_id) >= @MinDepartmentsCount", new { query.MinDepartmentsCount });
        }

        // 4. Безопасно определяем и добавляем сортировку
        string columnOrder = query.SortBy?.ToLower(CultureInfo.CurrentCulture) switch
        {
            "name" => "fl.name",
            "createdat" => "fl.created_at",
            _ => "fl.name"
        };
        string directionOrder = query.SortDirection?.ToLower(CultureInfo.CurrentCulture) == "desc" ? "DESC" : "ASC";

        // Передаем сортировку в OrderBy (SqlBuilder безопасно подставит строку в шаблон)
        builder.OrderBy($"{columnOrder} {directionOrder}");

        // 5. Добавляем параметры пагинации в контекст билдера
        int offset = (query.Page - 1) * query.PageSize;
        builder.AddParameters(new { query.PageSize, Offset = offset });

        try
        {
            using var connection = _sqlConnectionFactory.Create();

            long? totalCount = null;

            var result = (await connection.QueryAsync<LocationListItemDto, long, LocationListItemDto>(
                selector.RawSql,
                param: selector.Parameters,
                splitOn: "TotalCount",
                map: (item, count) =>
                {
                    totalCount ??= count;
                    return item;
                })).ToList();

            return new PagedList<LocationListItemDto>
            {
                Items = result,
                TotalCount = totalCount ?? 0,
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