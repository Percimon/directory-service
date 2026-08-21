using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.GetTop;

public sealed record GetLocationTopQuery() : IQuery;

public sealed class GetLocationTopQueryValidator : AbstractValidator<GetLocationTopQuery>
{
    public GetLocationTopQueryValidator()
    { }
}

public sealed class GetLocationTopHandler : IQueryHandler<IReadOnlyList<GetLocationTopResponse>, GetLocationTopQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IValidator<GetLocationTopQuery> _validator;
    private readonly ILogger<GetLocationTopHandler> _logger;

    public GetLocationTopHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetLocationTopQuery> validator,
        ILogger<GetLocationTopHandler> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<GetLocationTopResponse>, Error>> Handle(GetLocationTopQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        const string sql =
            """
            SELECT 
                l.id AS LocationId, 
                l.city, l.district, l.street, l.structure,
                COUNT(d.department_id) AS DepartmentsCount
            FROM locations AS l
            INNER JOIN department_locations AS d ON l.id = d.location_id
            GROUP BY 
                l.id, l.city, l.district, l.street, l.structure
            ORDER BY DepartmentsCount DESC
            LIMIT 5;
            """;

        using (var sqlConnection = _sqlConnectionFactory.Create())
        {
            var response = (await sqlConnection.QueryAsync<GetLocationTopResponse>(sql)).ToList();

            if (response is null || response.Count == 0)
            {
                _logger.LogError("No locations found.");

                return Error.NotFound("location", "No locations with departments found.");
            }

            _logger.LogInformation("Successfully retrieved top locations with departments.");

            return response;
        }

    }
}