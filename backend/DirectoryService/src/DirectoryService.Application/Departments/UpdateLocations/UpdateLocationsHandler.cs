using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationsHandler> _logger;
    private readonly IValidator<UpdateLocationsCommand> _validator;

    public UpdateLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ILogger<UpdateLocationsHandler> logger,
        IValidator<UpdateLocationsCommand> validator,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;

        var depId = DepartmentId.Create(command.DepartmentId);

        Result<Department, Error> depResult = await _departmentsRepository.GetByIdWithLocations(depId, cancellationToken);

        if (depResult.IsFailure)
        {
            transactionScope.Rollback();

            return depResult.Error;
        }

        List<LocationId> locationIds = command.LocationIds
            .Select(i => LocationId.Create(i))
            .ToList();

        UnitResult<Error> locationsAreExist = await _locationsRepository.LocationsExist(locationIds, cancellationToken);

        if (locationsAreExist.IsFailure)
        {
            transactionScope.Rollback();

            return locationsAreExist.Error;
        }

        depResult.Value.UpdateLocations(command.LocationIds);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();

            return saveResult.Error;
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();

            return commitResult.Error;
        }

        _logger.LogInformation("Locations were updated for department with id={Id}", depId.Value);

        return depId.Value;
    }
}
