using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.Locations)
            .NotEmpty()
            .Must(locations => locations.Distinct().Count() == locations.Count)
            .WithMessage("Location IDs list must not be empty and must not contain duplicates");
    }
}
