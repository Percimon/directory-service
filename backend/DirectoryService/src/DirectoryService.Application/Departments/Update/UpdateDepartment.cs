using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Update;

public record UpdateDepartmentCommand(
    Guid DepartmentId,
    string Name,
    string Slug) : ICommand;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());

        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}

public class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentCommand>
{
    private readonly IDepartmentsRepository _repository;
    private readonly IValidator<UpdateDepartmentCommand> _validator;
    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        IDepartmentsRepository repository,
        IValidator<UpdateDepartmentCommand> validator,
        ILogger<UpdateDepartmentHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(
        UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var departmentSearchResult = await _repository.GetById(command.DepartmentId, cancellationToken);

        if (departmentSearchResult.IsFailure)
        {
            return departmentSearchResult.Error;
        }

        Name name = Name.Create(command.Name).Value;

        Slug slug = Slug.Create(command.Slug).Value;

        var updateResult = departmentSearchResult.Value.UpdateMainInfo(name, slug);

        if (updateResult.IsFailure)
            return updateResult.Error;

        await _repository.Save(cancellationToken);

        _logger.LogInformation("Department with id={Id} now has Name={Name} and Slug={Slug}", command.DepartmentId, command.Name, command.Slug);

        return command.DepartmentId;
    }
}