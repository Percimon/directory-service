using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.Create;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.Create;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<CreateDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;

    }

    public async Task<Result<Guid, Error>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        DepartmentId departmentId = DepartmentId.New().Value;

        Name name = Name.Create(command.Name).Value;

        Slug identifier = Slug.Create(command.Slug).Value;

        List<LocationId> locationIds = command.Locations
                    .Select(i => LocationId.Create(i))
                    .ToList();

        UnitResult<Error> locationsAreExist = await _locationsRepository.LocationsExist(locationIds, cancellationToken);

        if (locationsAreExist.IsFailure)
        {
            return locationsAreExist.Error;
        }

        IEnumerable<DepartmentLocation> departmentLocations = command.Locations
            .Select(id => DepartmentLocation.Create(departmentId, LocationId.Create(id)).Value);

        if (command.ParentId is null)
        {
            var departmentResult = Department.CreateParent(
                name,
                identifier,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var addResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (addResult.IsFailure)
                return addResult.Error;

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            _logger.LogInformation("Department created with id={Id}", departmentId.Value);

            return departmentResult.Value.Id.Value;
        }
        else
        {
            var parentQuery = await _departmentsRepository.GetById(command.ParentId, cancellationToken);

            if (parentQuery.IsFailure)
                return parentQuery.Error;

            var departmentResult = Department.CreateChild(
                name,
                identifier,
                parentQuery.Value,
                departmentLocations,
                departmentId);

            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var addResult = await _departmentsRepository.Add(departmentResult.Value, cancellationToken);

            if (addResult.IsFailure)
                return addResult.Error;

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            _logger.LogInformation("Child department created with id={Id} for parent department with id={ParentId}", departmentId.Value, command.ParentId.Value);

            return departmentResult.Value.Id.Value;
        }
    }
}