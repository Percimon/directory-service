using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record Name
{
    private const int MIN_LENGTH = 3;

    private const int MAX_LENGTH = 150;

    private Name(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {

        }

        if (value.Length < MIN_LENGTH)
        {

        }

        if (value.Length > MAX_LENGTH)
        {

        }

        return new Name(value);
    }
}