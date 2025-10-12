using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Description
{
    private const int MAX_LENGTH = 10_000;

    private Description(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Description, Error> Create(string value)
    {
        if (value.Length > MAX_LENGTH)
        {
            return Errors.General.ValueIsInvalid(nameof(Description));
        }

        return new Description(value);
    }
}