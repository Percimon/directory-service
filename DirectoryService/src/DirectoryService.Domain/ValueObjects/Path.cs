using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Path
{
    private const int MIN_LENGTH = 3;

    private const int MAX_LENGTH = 150;

    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.General.ValueIsRequired(nameof(Path));
        }

        if (value.Length < MIN_LENGTH)
        {
            return Errors.General.ValueIsInvalid(nameof(Path));
        }

        if (value.Length > MAX_LENGTH)
        {
            return Errors.General.ValueIsInvalid(nameof(Path));
        }

        return new Path(value);
    }
}