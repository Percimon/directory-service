using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Name
{
    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(Name));
        }

        if (value.Length is < Constants.TextLength.LENGTH_3 or > Constants.TextLength.LENGTH_150)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Name));
        }

        return new Name(value);
    }
}