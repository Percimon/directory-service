using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly IValidator<CreatePositionCommand> _validator;

    public CreatePositionHandler(
        IPositionsRepository postionsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionCommand> validator,
        ITransactionManager transactionManager)
    {
        _positionsRepository = postionsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        PositionId positionId = PositionId.New();

        Name name = Name.Create(command.Name).Value;

        Description description = Description.Create(command.Description).Value;

        List<DepartmentId> departmentIds = command.Departments
           .Select(i => DepartmentId.Create(i))
           .ToList();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;

        UnitResult<Error> departmentsAreExist = await _departmentsRepository.DepartmentsExist(departmentIds, cancellationToken);

        if (departmentsAreExist.IsFailure)
        {
            transactionScope.Rollback();

            return departmentsAreExist.Error;
        }

        DateTime createdAt = DateTime.UtcNow;

        var position = new Position(positionId, name, description, createdAt);

        var addResult = await _positionsRepository.Add(position, cancellationToken);

        if (addResult.IsFailure)
        {
            transactionScope.Rollback();

            return addResult.Error;
        }

        foreach (Guid depId in command.Departments)
        {
            var id = DepartmentId.Create(depId);

            var dep = await _departmentsRepository.GetByIdWithPositions(id, cancellationToken);

            if (dep.IsFailure)
            {
                transactionScope.Rollback();

                return dep.Error;
            }

            var depValue = dep.Value;

            var addPositionResult = depValue.AddPosition(positionId.Value);

            if (addPositionResult.IsFailure)
            {
                transactionScope.Rollback();

                return addPositionResult.Error;
            }
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to create Position with name={Name}. Error: {Error}", command.Name, saveResult.Error);

            return saveResult.Error;
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            _logger.LogError("Failed to commit transaction for Position with name={Name}. Error: {Error}", command.Name, commitResult.Error);

            return commitResult.Error;
        }

        return position.Id.Value;
    }
}
