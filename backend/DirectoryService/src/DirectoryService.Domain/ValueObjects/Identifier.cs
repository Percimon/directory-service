using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Identifier
{
    private static readonly Regex _identifierRegex = new("^[a-zA-Z]+$", RegexOptions.Compiled);

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(Identifier));
        }

        if (value.Length is < Constants.TextLength.LENGTH_3 or > Constants.TextLength.LENGTH_150)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Identifier));
        }

        if (!_identifierRegex.IsMatch(value))
        {
            return GeneralErrors.EnglishCharactersOnly(nameof(Identifier));
        }

        return new Identifier(value);
    }
}