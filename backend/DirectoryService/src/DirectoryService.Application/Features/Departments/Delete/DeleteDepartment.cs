using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Identifiers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Features.Departments.Delete;

public sealed record DeleteDepartmentCommand(Guid Id) : ICommand;

public sealed class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Department Id is required.");
    }
}

public sealed class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<DeleteDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        IValidator<DeleteDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var searchResult = await _departmentsRepository.GetById(DepartmentId.Create(command.Id), cancellationToken);

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

        return Result.Success<Guid, Error>(command.Id);
    }
}