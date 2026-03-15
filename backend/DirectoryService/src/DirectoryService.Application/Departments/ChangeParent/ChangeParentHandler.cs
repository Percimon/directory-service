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
            return lockDescendantsResult.Error;
        }

        if (command.NewParentId is not null)
        {
            var newParent = await _departmentsRepository.GetByIdWithLock(command.NewParentId, cancellationToken);

            if (newParent.IsFailure)
                return newParent.Error;

            string newParentPath = newParent.Value.Path.Value;

            if (newParentPath == oldPath || newParentPath.StartsWith($"{oldPath}."))
            {
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