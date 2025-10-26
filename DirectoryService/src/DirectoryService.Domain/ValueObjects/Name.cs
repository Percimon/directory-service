using CSharpFunctionalExtensions;
using SharedKernel;

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
            return Errors.General.ValueIsRequired(nameof(Name));
        }

        if (value.Length < Constants.TextLength.LENGTH_3)
        {
            return Errors.General.ValueIsInvalid(nameof(Name));
        }

        if (value.Length > Constants.TextLength.LENGTH_150)
        {
            return Errors.General.ValueIsInvalid(nameof(Name));
        }

        return new Name(value);
    }
}