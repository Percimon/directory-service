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
    private readonly IValidator<ChangeParentCommand> _validator;
    private readonly ILogger<ChangeParentHandler> _logger;

    public ChangeParentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        IValidator<ChangeParentCommand> validator,
        ILogger<ChangeParentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
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

        var queryResult = await _departmentsRepository.GetByIdWithLock(command.DepartmentId, cancellationToken);

        if (queryResult.IsFailure)
        {
            transactionScope.Rollback();

            return queryResult.Error;
        }

        string oldPath = queryResult.Value.Path.Value;

        string newPath = string.Empty;

        var lockDescendantsResult = await _departmentsRepository.LockDescendants(oldPath, cancellationToken);

        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();

            return lockDescendantsResult.Error;
        }

        if (command.NewParentId is not null)
        {
            var newParent = await _departmentsRepository.GetByIdWithLock(command.NewParentId, cancellationToken);

            if (newParent.IsFailure)
            {
                transactionScope.Rollback();

                return newParent.Error;
            }

            string newParentPath = newParent.Value.Path.Value;

            if (newParentPath == oldPath || newParentPath.StartsWith($"{oldPath}."))
            {
                transactionScope.Rollback();

                return GeneralErrors.Failure("New parent can't be child of current parent");
            }

            newPath = newParentPath;
        }

        var updateResult = await _departmentsRepository.ChangeParent(
            oldPath,
            newPath,
            command.DepartmentId,
            command.NewParentId,
            cancellationToken);

        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();

            return updateResult.Error;
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

        _logger.LogInformation("Родитель  отдела с Id={id} обновлен, включая его дочерние сущности", command.DepartmentId);

        return command.DepartmentId;
    }
}