using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Positions.Delete;

public sealed record DeletePositionCommand(Guid Id) : ICommand;

public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Position ID is required.");
    }
}

public sealed class DeletePositionHandler : ICommandHandler<Guid, DeletePositionCommand>
{
    private readonly IPositionsRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<DeletePositionCommand> _validator;
    private readonly ILogger<DeletePositionHandler> _logger;

    public DeletePositionHandler(
        IPositionsRepository positionRepository,
        ITransactionManager transactionManager,
        IValidator<DeletePositionCommand> validator,
        ILogger<DeletePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(DeletePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var searchResult = await _positionRepository.GetById(PositionId.Create(command.Id), cancellationToken);

        if (searchResult.IsFailure)
        {
            return searchResult.Error;
        }

        var deleteResult = searchResult.Value.SoftDelete();

        if (deleteResult.IsFailure)
            return deleteResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        _logger.LogInformation("Position with ID: {PositionId} has been deleted successfully.", command.Id);

        return command.Id;
    }
}