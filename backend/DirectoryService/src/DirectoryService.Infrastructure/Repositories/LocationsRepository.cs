using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("name", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GeneralErrors.AlreadyExists(nameof(Location), nameof(Location.Name), location.Name.Value);
                }
                else if (pgEx.ConstraintName.Contains("address", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GeneralErrors.AlreadyExists(nameof(Location), nameof(Location.Address), location.Address.ToString());
                }
            }

            _logger.LogError(ex, "Database update error while creating location with name: {name}", location.Name.Value);

            return GeneralErrors.Failure("Database update error while creating location");
        }
        catch (Exception e)
        {
            string message = "Failed to insert location";

            _logger.LogError(e, message);

            return Error.Failure("location.insert", message);
        }
    }

    public UnitResult<Error> LocationExists(LocationId id)
    {
        var query = _dbContext.Locations.FirstOrDefault(l => id == l.Id && l.IsActive);

        if (query is null)
            return GeneralErrors.NotFound(id.Value);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> LocationsExist(IEnumerable<LocationId> ids, CancellationToken cancellationToken)
    {
        try
        {
            if (ids is null)
                return Error.NotFound("location.id", "Locations id list is null");

            LocationId[] locationIds = ids.ToArray();

            int expectedCount = locationIds.Length;

            if (expectedCount == 0)
                return Error.NotFound("location.id", "Locations id list is empty");

            int count = await _dbContext.Locations
                .Where(l => Array.IndexOf(locationIds, l.Id) > -1 && l.IsActive)
                .CountAsync(cancellationToken);

            return expectedCount == count
                ? UnitResult.Success<Error>()
                : Error.NotFound("location.id", "One of location ids were not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("database", "Locations id count failed");
        }
    }
}