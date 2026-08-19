using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.ChangeParent;

public class ChangeParentCommandValidator : AbstractValidator<ChangeParentCommand>
{
    public ChangeParentCommandValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty()
            .WithError(Error.Validation("validation", "DepartmentId is required"));

        RuleFor(c => c.NewParentId)
            .NotEmpty()
            .WithError(Error.Validation("validation", "NewParentId is required"));

        RuleFor(c => c)
            .Must(command => command.DepartmentId != command.NewParentId)
            .WithError(Error.Conflict("validation", "Department can't be parent of itself"));
    }
}
