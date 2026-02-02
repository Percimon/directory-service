using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.ChangeParent;

public class ChangeParentCommandValidator : AbstractValidator<ChangeParentCommand>
{
    public ChangeParentCommandValidator()
    {
        RuleFor(c => c)
            .Must(command => command.DepartmentId != command.NewParentId)
            .WithError(Error.Conflict("validation", "Department can'te be parent of itself"));
    }
}
