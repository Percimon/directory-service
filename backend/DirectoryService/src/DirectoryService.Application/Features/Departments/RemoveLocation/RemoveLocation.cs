using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.RemoveLocation;

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
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RemoveLocationHandler> _logger;

    public RemoveLocationHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<RemoveLocationCommand> validator,
        ITransactionManager transactionManager,
        ILogger<RemoveLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
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

        var removeLocationResult = departmentSearchResult.Value.RemoveLocation(command.LocationId);

        if (removeLocationResult.IsFailure)
            return removeLocationResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            _logger.LogError("Failed to remove Location with Id={LocationId} from Department with Id={DepartmentId}. Error: {Error}", command.LocationId, command.DepartmentId, saveResult.Error);

            return saveResult.Error;
        }

        _logger.LogInformation("Location with Id={LocationId} removed from Department with Id={DepartmentId}", command.LocationId, command.DepartmentId);

        return command.LocationId;
    }
}