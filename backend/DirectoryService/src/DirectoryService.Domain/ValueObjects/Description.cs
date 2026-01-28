using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Description
{
    private Description(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Description, Error> Create(string value)
    {
        if (value.Length > Constants.TextLength.LENGTH_1000)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Description));
        }

        return new Description(value);
    }
}