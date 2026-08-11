using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Slug
{
    private static readonly Regex _identifierRegex = new("^[a-zA-Z]+$", RegexOptions.Compiled);

    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Slug, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(nameof(Slug));
        }

        if (value.Length is < Constants.TextLength.LENGTH_3 or > Constants.TextLength.LENGTH_150)
        {
            return GeneralErrors.ValueIsInvalid(nameof(Slug));
        }

        if (!_identifierRegex.IsMatch(value))
        {
            return GeneralErrors.EnglishCharactersOnly(nameof(Slug));
        }

        return new Slug(value);
    }
}