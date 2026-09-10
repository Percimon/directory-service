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

    }
}
