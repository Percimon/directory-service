using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Path
{
    private const char SEPARATOR = '/';

    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Path CreateParent(Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return new Path(identifier.Value);
    }

    public Path CreateChild(Identifier childIdentifier)
    {
        ArgumentNullException.ThrowIfNull(childIdentifier);

        return new Path(Value + SEPARATOR + childIdentifier!.Value);
    }
}