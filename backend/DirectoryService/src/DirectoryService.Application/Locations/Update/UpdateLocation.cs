using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone) : ICommand;

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.Name)
                   .MustBeValueObject(Name.Create);

        RuleFor(x => new { x.City, x.District, x.Street, x.Structure })
            .MustBeValueObject(a => Address.Create(a.City, a.District, a.Street, a.Structure));

        RuleFor(x => x.TimeZone)
            .MustBeValueObject(TimeZone.Create);
    }
}

public class UpdateLocationHandler : ICommandHandler<Guid, UpdateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly IValidator<UpdateLocationCommand> _validator;
    private readonly ILogger<UpdateLocationHandler> _logger;

    public UpdateLocationHandler(
        ILocationsRepository repository,
        IValidator<UpdateLocationCommand> validator,
        ILogger<UpdateLocationHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var id = LocationId.Create(command.Id);

        var locationSearchResult = await _repository.GetById(id, cancellationToken);

        if (locationSearchResult.IsFailure)
            return locationSearchResult.Error;

        var name = Name.Create(command.Name).Value;

        var address = Address.Create(command.City, command.District, command.Street, command.Structure).Value;

        var timeZone = TimeZone.Create(command.TimeZone).Value;

        var result = locationSearchResult.Value.UpdateMainInfo(name, address, timeZone);

        if (result.IsFailure)
            return result.Error;

        var saveResult = await _repository.Save(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogInformation("Location with Id={Id} was updated.", command.Id);

        return command.Id;
    }
}
