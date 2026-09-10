using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Entities;
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

        Department? department = null;
        Department? newParent = null;

        var idsToLock = new[]
            {
                command.DepartmentId,
                command.NewParentId,
            }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .OrderBy(id => id);

        foreach (var id in idsToLock)
        {
            var queryResult = await _departmentsRepository.GetByIdWithLock(id, cancellationToken);

            if (queryResult.IsFailure)
            {
                transactionScope.Rollback();

                return queryResult.Error;
            }

            if (id == command.DepartmentId)
            {
                department = queryResult.Value;
            }
            else
            {
                newParent = queryResult.Value;
            }
        }

        string currentPath = department!.Path.Value;

        string newPath = string.Empty;

        var lockDescendantsResult = await _departmentsRepository.LockDescendants(currentPath, cancellationToken);

        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();

            return lockDescendantsResult.Error;
        }

        if (command.DepartmentId == command.NewParentId)
        {
            transactionScope.Rollback();

            return Error.Conflict("department.move.parent_is_self", "Department can't be parent of itself");
        }

        if (command.NewParentId is not null)
        {
            if (department.Parent?.Id == command.NewParentId)
            {
                transactionScope.Rollback();

                return new ChangeParentResponseDto(
                    department.Id.Value,
                    department.Parent?.Id.Value,
                    department.Path.Value,
                    department.Depth.Value,
                    department.UpdatedAt);
            }

            string newParentPath = newParent!.Path.Value;

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