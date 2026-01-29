using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;
using TimeZone = DirectoryService.Domain.ValueObjects.TimeZone;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => new { x.City, x.District, x.Street, x.Structure })
            .MustBeValueObject(a => Address.Create(a.City, a.District, a.Street, a.Structure));

        RuleFor(x => x.TimeZone)
            .MustBeValueObject(TimeZone.Create);
    }
}