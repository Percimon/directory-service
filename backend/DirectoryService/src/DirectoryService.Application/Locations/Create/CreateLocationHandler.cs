using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationHandler(
        ILocationsRepository repository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationCommand> validator)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            validationResult.ToError();
        }

        var locationId = LocationId.New();

        var name = Name.Create(command.Name);

        var address = Address.Create(
            command.City,
            command.District,
            command.Street,
            command.Structure);

        var timeZone = TimeZone.Create(command.TimeZone);

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
