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

    public static Result<Path, Error> CreateParent(Identifier identifier)
    {
        if (identifier is null)
        {
            return Error.Validation("identifier", "Identifier cant be null");
        }

        return new Path(identifier.Value);
    }

    public Result<Path, Error> CreateChild(Identifier childIdentifier)
    {
        if (childIdentifier is null)
        {
            return Error.Validation("identifier", "Identifier cant be null");
        }

        return new Path(Value + SEPARATOR + childIdentifier!.Value);
    }
}