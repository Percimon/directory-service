using DirectoryService.Application.Departments.Create;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Create;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Identifier)
            .MustBeValueObject(Slug.Create);

        RuleFor(x => x.Locations)
            .NotEmpty()
            .WithError(GeneralErrors.Failure("Location IDs list should not be empty"));

        RuleFor(x => x.Locations)
            .Must(locations => locations.Distinct().Count() == locations.Count)
            .WithError(GeneralErrors.Failure("Location IDs list should contain unique IDs"));

    }
}
