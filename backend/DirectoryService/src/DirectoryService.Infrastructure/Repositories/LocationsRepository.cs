using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly AppDbContext _dbContext;

    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(
        AppDbContext dbContext,
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

            _logger.LogError(pgEx, "Database update error while creating location with name: {name}", location.Name.Value);

            return Error.Failure("location.add", pgEx.Message);
        }
        catch (Exception e)
        {
            string message = "Failed to insert location";

            _logger.LogError(e, message);

            return Error.Failure("location.add", e.Message);
        }
    }

    public async Task<Result<Location, Error>> GetById(LocationId id, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _dbContext.Locations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (query is null)
                return GeneralErrors.NotFound(id.Value);

            return query;
        }
        catch (PostgresException pEx)
        {
            _logger.LogError(pEx, "Ошибка работы с БД");

            return Error.Failure("location.get", pEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка");

            return Error.Failure("location.get", ex.Message);
        }
    }

    public async Task<UnitResult<Error>> LocationExists(LocationId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _dbContext.Locations.FirstOrDefaultAsync(l => id == l.Id && l.IsActive, cancellationToken);

            if (query is null)
                return GeneralErrors.NotFound(id.Value);

            return UnitResult.Success<Error>();
        }
        catch (PostgresException pEx)
        {
            _logger.LogError(pEx, "Ошибка работы с БД");

            return Error.Failure("location.exists", pEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка");

            return Error.Failure("location.exists", ex.Message);
        }
    }

    public async Task<UnitResult<Error>> LocationNameExists(Name name, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _dbContext.Locations.FirstOrDefaultAsync(l => name.Value == l.Name.Value && l.IsActive, cancellationToken);

            if (query is not null)
                return GeneralErrors.AlreadyExists(name.Value);

            return UnitResult.Success<Error>();
        }
        catch (PostgresException pEx)
        {
            _logger.LogError(pEx, "Ошибка работы с БД");

            return Error.Failure("location.name_exists", pEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка");

            return Error.Failure("location.name_exists", ex.Message);
        }
    }

    public async Task<UnitResult<Error>> LocationsExist(IEnumerable<LocationId> ids, CancellationToken cancellationToken = default)
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
        catch (PostgresException pEx)
        {
            _logger.LogError(pEx, "Ошибка работы с БД");

            return Error.Failure("location.id_list_exists", pEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка");

            return Error.Failure("location.id_list_exists", ex.Message);
        }
    }

    public async Task<UnitResult<Error>> Save(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            string message = "Failed to save changes";

            _logger.LogError(e, message);

            return Error.Failure("database.save", message);
        }
    }
}