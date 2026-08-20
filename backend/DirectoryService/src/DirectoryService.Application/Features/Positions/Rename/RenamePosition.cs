using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Positions.Rename;

public sealed record RenamePositionCommand(Guid PositionId, string NewName) : ICommand;

public sealed class RenamePositionCommandValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required.");

        RuleFor(x => x.NewName)
            .MustBeValueObject(x => Name.Create(x));
    }
}

public sealed class RenamePositionHandler : ICommandHandler<Guid, RenamePositionCommand>
{
    private readonly IPositionsRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<RenamePositionCommand> _validator;
    private readonly ILogger<RenamePositionHandler> _logger;

    public RenamePositionHandler(
        IPositionsRepository positionRepository,
        ITransactionManager transactionManager,
        IValidator<RenamePositionCommand> validator,
        ILogger<RenamePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(RenamePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var positionId = PositionId.Create(command.PositionId);

        var positionResult = await _positionRepository.GetById(positionId, cancellationToken);

        if (positionResult.IsFailure)
        {
            return positionResult.Error;
        }

        var newName = Name.Create(command.NewName).Value;

        var renameResult = positionResult.Value.Rename(newName);

        if (renameResult.IsFailure)
        {
            return renameResult.Error;
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Position renamed successfully: {PositionId}", command.PositionId);

        return Result.Success<Guid, Error>(command.PositionId);
    }
}