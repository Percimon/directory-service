using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Description)
            .MustBeValueObject(Description.Create);

        RuleFor(x => x.Departments)
            .NotEmpty()
            .Must(departments => departments.Distinct().Count() == departments.Count)
            .WithMessage("Department IDs list must not be empty and must not contain duplicates");
    }
}