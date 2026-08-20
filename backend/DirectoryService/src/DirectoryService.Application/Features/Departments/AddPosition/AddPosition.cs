using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.AddPosition;

public sealed record AddPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;

public sealed class AddPositionCommandValidator : AbstractValidator<AddPositionCommand>
{
    public AddPositionCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());
    }
}

public sealed class AddPositionHandler : ICommandHandler<Guid, AddPositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AddPositionHandler> _logger;

    public AddPositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<AddPositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(AddPositionCommand command, CancellationToken cancellationToken)
    {
        var departmentSearchResult = await _departmentsRepository.GetByIdWithPositions(DepartmentId.Create(command.DepartmentId), cancellationToken);
        if (departmentSearchResult.IsFailure)
            return departmentSearchResult.Error;

        var positionSearchResult = await _positionsRepository.GetById(PositionId.Create(command.PositionId), cancellationToken);
        if (positionSearchResult.IsFailure)
            return positionSearchResult.Error;

        var department = departmentSearchResult.Value;

        var addPositionResult = department.AddPosition(command.PositionId);
        if (addPositionResult.IsFailure)
            return addPositionResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        _logger.LogInformation("Position {PositionId} added to department {DepartmentId}", command.PositionId, command.DepartmentId);

        return command.PositionId;
    }
}