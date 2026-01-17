using DirectoryService.Application.Positions.Create;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel;
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
            .WithError(GeneralErrors.ValueIsInvalid("Department IDs list"));
    }
}