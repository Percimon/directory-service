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
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationsQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetLocationsHandler> _logger;

    public GetLocationsHandler(
        IReadDbContext readDbContext,
        IValidator<GetLocationsQuery> validator,
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<GetLocationsHandler> logger)
    {
        _readDbContext = readDbContext;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<PagedList<LocationListItemDto>, Error>> Handle(
        GetLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        const string sqlQuery =
        """
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
            WHERE 
                (@Search IS NULL OR LOWER(l.name) LIKE LOWER('%' || @Search || '%'))
            GROUP BY 
                l.id, l.name, l.city, l.district, l.street, l.structure, l.created_at
            HAVING 
                (@MinDepartmentsCount IS NULL OR COUNT(d.department_id) >= @MinDepartmentsCount)
        )
        SELECT 
            fl.id,
            fl.name,
            fl.city, fl.district, fl.street, fl.structure,
            fl.created_at,
            fl.DepartmentsCount,
            fl.TotalCount
        FROM filtered_locations AS fl
        ORDER BY
            CASE WHEN @SortBy = 'name' AND @SortDirection = 'asc' THEN fl.name END ASC,
            CASE WHEN @SortBy = 'name' AND @SortDirection = 'desc' THEN fl.name END DESC,
            CASE WHEN @SortBy = 'createdAt' AND @SortDirection = 'asc' THEN fl.created_at END ASC,
            CASE WHEN @SortBy = 'createdAt' AND @SortDirection = 'desc' THEN fl.created_at END DESC
        LIMIT @PageSize OFFSET (@Page - 1) * @PageSize;
        """;

        try
        {
            using var connection = _sqlConnectionFactory.Create();

            long? totalCount = null;

            var result = (await connection.QueryAsync<LocationListItemDto, long, LocationListItemDto>(
                sqlQuery,
                splitOn: "TotalCount",
                map: (item, count) =>
                {
                    totalCount ??= count;
                    return item;
                },
                param: new
                {
                    request.Page,
                    request.PageSize,
                    request.SortBy,
                    request.SortDirection,
                    Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search,
                    MinDepartmentsCount = request.MinDepartmentsCount > 0 ? (int?)request.MinDepartmentsCount : null,
                })).ToList();

            return new PagedList<LocationListItemDto>
            {
                Items = result,
                TotalCount = totalCount ?? 0,
                Page = request.Page,
                PageSize = request.PageSize,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving locations.");

            return Error.Failure("location.get", "An error occurred while retrieving locations.");
        }
    }
}