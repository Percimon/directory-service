using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation.Results;
using SharedKernel;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace DirectoryService.Application.Validation
{
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
}