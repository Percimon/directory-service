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
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateLocationsHandler> _logger;
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public UpdateLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILogger<UpdateLocationsHandler> logger,
        IValidator<UpdateLocationsCommand> validator,
        ISqlConnectionFactory sqlConnectionFactory,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
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

        bool locationsAreExist = AllIdsAreExist(command.LocationIds);

        if (!locationsAreExist)
        {
            transactionScope.Rollback();

            return GeneralErrors.NotFound();
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

    private bool AllIdsAreExist(IReadOnlyList<Guid> ids)
    {
        DynamicParameters parameters = new DynamicParameters();

        parameters.Add("@Ids", ids);

        string query =
            """
                SELECT COUNT(id) 
                FROM locations 
                WHERE id = ANY(@Ids) AND is_active = TRUE;
            """;

        using (var connection = _sqlConnectionFactory.Create())
        {
            int count = connection.ExecuteScalar<int>(query, parameters);

            return count == ids.Count;
        }
    }
}
