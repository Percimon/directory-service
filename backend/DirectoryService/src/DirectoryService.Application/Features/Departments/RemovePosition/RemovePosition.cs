using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.RemovePosition;

public sealed record RemovePositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;

public sealed class RemovePositionCommandValidator : AbstractValidator<RemovePositionCommand>
{
    public RemovePositionCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());
    }
}

public sealed class RemovePositionHandler : ICommandHandler<Guid, RemovePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RemovePositionHandler> _logger;

    public RemovePositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<RemovePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(RemovePositionCommand command, CancellationToken cancellationToken)
    {
        var departmentSearchResult = await _departmentsRepository.GetByIdWithPositions(DepartmentId.Create(command.DepartmentId), cancellationToken);
        if (departmentSearchResult.IsFailure)
            return departmentSearchResult.Error;

        var positionSearchResult = await _positionsRepository.GetById(PositionId.Create(command.PositionId), cancellationToken);
        if (positionSearchResult.IsFailure)
            return positionSearchResult.Error;

        var department = departmentSearchResult.Value;
        var position = positionSearchResult.Value;

        var removePositionResult = department.RemovePosition(command.PositionId);
        if (removePositionResult.IsFailure)
            return removePositionResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogInformation("Position {PositionId} removed from department {DepartmentId}", command.PositionId, command.DepartmentId);

        return command.PositionId;
    }
}