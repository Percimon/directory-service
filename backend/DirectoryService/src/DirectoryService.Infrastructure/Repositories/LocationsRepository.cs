using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
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
            catch (Exception e)
            {
                string message = "Failed to insert location";

                _logger.LogError(e, message);

                return Error.Failure("location.insert", message);
            }
        }
    }
}