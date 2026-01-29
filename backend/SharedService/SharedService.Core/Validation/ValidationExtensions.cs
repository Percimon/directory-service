using System.Text.Json;
using SharedService.SharedKernel;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SharedService.Core.Validation;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult validationResult)
    {
        IEnumerable<ErrorMessage> errorMessages = validationResult
            .Errors
            .SelectMany(e =>
            {
                return JsonSerializer.Deserialize<Error>(e.ErrorMessage).Messages;
            });

        return Error.Validation(errorMessages);
    }
}