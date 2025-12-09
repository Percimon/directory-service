using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create
{
    public class CreateLocationHandler
    {
        private readonly ILocationsRepository _repository;
        private readonly ILogger<CreateLocationHandler> _logger;

        public CreateLocationHandler(
            ILocationsRepository repository,
            ILogger<CreateLocationHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<Guid, Error>> Handle(
            CreateLocationCommand command,
            CancellationToken cancellationToken = default)
        {
            var locationId = LocationId.New();

            var name = Name.Create(command.Name);

            if (name.IsFailure)
                return name.Error;

            var address = Address.Create(
                command.City,
                command.District,
                command.Street,
                command.Structure);

            if (address.IsFailure)
                return address.Error;

            var timeZone = TimeZone.Create(command.TimeZone);

            if (timeZone.IsFailure)
                return timeZone.Error;

            var dateTime = DateTime.UtcNow;

            var location = new Location(
                locationId,
                name.Value,
                address.Value,
                timeZone.Value,
                dateTime);

            var result = await _repository.Add(location, cancellationToken);

            if (result.IsFailure)
                return result.Error;

            _logger.LogInformation("Location created with id={Id}", location.Id.Value);

            return location.Id.Value;
        }
    }
}