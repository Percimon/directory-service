using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsCommandValidator : AbstractValidator<UpdateLocationsCommand>
{
    public UpdateLocationsCommandValidator()
    {
        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .Must(locations => locations.Distinct().Count() == locations.Count)
            .WithError(GeneralErrors.ValueIsInvalid("Location IDs list"));
    }
}