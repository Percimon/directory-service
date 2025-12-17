using System.Data.Common;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel;

namespace DirectoryService.Infrastructure.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<LocationsRepository> _logger;

        public LocationsRepository(DirectoryServiceDbContext dbContext, ILogger<LocationsRepository> logger)
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
    }
}