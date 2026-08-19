using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.AddLocation;

public sealed record AddLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand;

public sealed class AddLocationCommandValidator : AbstractValidator<AddLocationCommand>
{
    public AddLocationCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());
    }
}

public sealed class AddLocationHandler : ICommandHandler<Guid, AddLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<AddLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AddLocationHandler> _logger;

    public AddLocationHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<AddLocationCommand> validator,
        ITransactionManager transactionManager,
        ILogger<AddLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(AddLocationCommand command, CancellationToken cancellationToken)
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

        var addLocationResult = departmentSearchResult.Value.AddLocation(command.LocationId);

        if (addLocationResult.IsFailure)
            return addLocationResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        _logger.LogInformation("Location with Id={LocationId} added to Department with Id={DepartmentId}", command.LocationId, command.DepartmentId);

        return command.LocationId;
    }
}