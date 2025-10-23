using System.Reflection.Metadata;
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

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {

        }

        if (value.Length < Constants.TextLength.LENGTH_3)
        {

        }

        if (value.Length > Constants.TextLength.LENGTH_150)
        {

        }

        return new Name(value);
    }
}