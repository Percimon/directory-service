using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository repository,
        IValidator<CreateLocationCommand> validator,
        ITransactionManager transactionManager,
        ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var name = Name.Create(command.Name);

        var nameExistenceResult = await _repository.LocationNameExists(name.Value, cancellationToken);

        if (nameExistenceResult.IsFailure)
        {
            return nameExistenceResult.Error;
        }

        var locationId = LocationId.New();

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

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            _logger.LogError("Failed to create Location with name={Name}. Error: {Error}", command.Name, saveResult.Error);

            return saveResult.Error;
        }

        _logger.LogInformation("Location created with id={Id}", location.Id.Value);

        return location.Id.Value;
    }
}