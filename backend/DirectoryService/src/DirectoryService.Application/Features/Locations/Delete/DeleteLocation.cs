using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Locations.Delete;

public sealed record DeleteLocationCommand(Guid Id) : ICommand;

public sealed class DeleteLocationCommandValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Location ID is required.");
    }
}

public sealed class DeleteLocationHandler : ICommandHandler<Guid, DeleteLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<DeleteLocationCommand> _validator;
    private readonly ILogger<DeleteLocationHandler> _logger;

    public DeleteLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<DeleteLocationCommand> validator,
        ILogger<DeleteLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var deleteResult = await _locationsRepository.Delete(LocationId.Create(command.Id), cancellationToken);

        if (deleteResult.IsFailure)
        {
            return deleteResult.Error;
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        _logger.LogInformation("Location with ID: {LocationId} has been deleted successfully.", command.Id);

        return command.Id;
    }
}