using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.ChangeParent;

public class ChangeParentHandler : ICommandHandler<Guid, ChangeParentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<ChangeParentHandler> _logger;
    private readonly IValidator<ChangeParentCommand> _validator;

    public ChangeParentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<ChangeParentHandler> logger,
        IValidator<ChangeParentCommand> validator)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Error>> Handle(
        ChangeParentCommand command,
        CancellationToken cancellationToken)
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

        var currentDepartment = await _departmentsRepository.GetByIdWithLock(command.DepartmentId, cancellationToken);

        if (currentDepartment.IsFailure)
            return currentDepartment.Error;

        if (command.NewParentId is not null)
        {
            var newParentExistsResult = await _departmentsRepository.DepartmentsExist([command.NewParentId], cancellationToken);

            if (newParentExistsResult.IsFailure)
            {
                return Error.NotFound("department.database", "New parent was not found");
            }

            // Нельзя выбрать своё "дочернее" подразделение
        }

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

        return command.DepartmentId;
    }
}