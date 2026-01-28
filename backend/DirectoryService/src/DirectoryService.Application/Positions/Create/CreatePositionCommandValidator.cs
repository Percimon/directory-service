using DirectoryService.Application.Positions.Create;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

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