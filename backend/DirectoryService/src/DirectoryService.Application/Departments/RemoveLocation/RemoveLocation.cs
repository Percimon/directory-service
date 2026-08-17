using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

public sealed record RemoveLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand;

public sealed class RemoveLocationCommandValidator : AbstractValidator<RemoveLocationCommand>
{
    public RemoveLocationCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());
    }
}

public sealed class RemoveLocationHandler : ICommandHandler<Guid, RemoveLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<RemoveLocationCommand> _validator;
    private readonly ILogger<RemoveLocationHandler> _logger;

    public RemoveLocationHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<RemoveLocationCommand> validator,
        ILogger<RemoveLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(RemoveLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var departmentSearchResult = await _departmentsRepository.GetByIdWithLocations(DepartmentId.Create(command.DepartmentId), cancellationToken);
        if (departmentSearchResult.IsFailure)
            return departmentSearchResult.Error;

        var locationSearchResult = await _locationsRepository.GetById(LocationId.Create(command.LocationId), cancellationToken);
        if (locationSearchResult.IsFailure)
            return locationSearchResult.Error;

        var RemoveLocationResult = departmentSearchResult.Value.RemoveLocation(command.LocationId);

        if (RemoveLocationResult.IsFailure)
            return RemoveLocationResult.Error;

        await _departmentsRepository.Save(cancellationToken);

        _logger.LogInformation("Location with Id={LocationId} removed from Department with Id={DepartmentId}", command.LocationId, command.DepartmentId);

        return command.LocationId;
    }
}