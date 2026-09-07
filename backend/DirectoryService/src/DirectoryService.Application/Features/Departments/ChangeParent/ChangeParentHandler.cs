using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.ChangeParent;

public class ChangeParentHandler : ICommandHandler<ChangeParentResponseDto, ChangeParentCommand>
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

    public async Task<Result<ChangeParentResponseDto, Error>> Handle(
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

        string currentPath = queryResult.Value.Path.Value;

        string newPath = string.Empty;

        var lockDescendantsResult = await _departmentsRepository.LockDescendants(currentPath, cancellationToken);

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

            if (queryResult.Value.Parent?.Id == command.NewParentId)
            {
                transactionScope.Rollback();

                return new ChangeParentResponseDto(
                    queryResult.Value.Id.Value,
                    queryResult.Value.Parent?.Id.Value,
                    queryResult.Value.Path.Value,
                    queryResult.Value.Depth.Value,
                    queryResult.Value.UpdatedAt);
            }

            string newParentPath = newParent.Value.Path.Value;

            if (newParentPath == currentPath || newParentPath.StartsWith($"{currentPath}.", StringComparison.Ordinal))
            {
                transactionScope.Rollback();

                return Error.Failure("department.move.cycle", "New parent can't be child of current parent");
            }

            newPath = newParentPath;
        }

        var updateResult = await _departmentsRepository.ChangeParent(
            currentPath,
            newPath,
            command.DepartmentId,
            command.NewParentId,
            cancellationToken);

        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();

            return updateResult.Error;
        }

        var updatedDepartmentResult = await _departmentsRepository.GetByIdWithLock(
            command.DepartmentId,
            cancellationToken);

        if (updatedDepartmentResult.IsFailure)
        {
            transactionScope.Rollback();

            return updatedDepartmentResult.Error;
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

        return new ChangeParentResponseDto(
            updatedDepartmentResult.Value.Id.Value,
            updatedDepartmentResult.Value.Parent?.Id.Value,
            updatedDepartmentResult.Value.Path.Value,
            updatedDepartmentResult.Value.Depth.Value,
            updatedDepartmentResult.Value.UpdatedAt);
    }
}