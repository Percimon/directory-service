using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Path
{
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

        if (value.Length < Constants.TextLength.LENGTH_3)
        {
            return Errors.General.ValueIsInvalid(nameof(Path));
        }

        if (value.Length > Constants.TextLength.LENGTH_150)
        {
            return Errors.General.ValueIsInvalid(nameof(Path));
        }

        return new Path(value);
    }
}