using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public class Identifier
{
    private const int MIN_LENGTH = 3;

    private const int MAX_LENGTH = 150;

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.General.ValueIsRequired(nameof(Identifier));
        }

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Errors.General.ValueIsInvalid(nameof(Identifier));
        }

        string pattern = @"^[a-zA-Z]+$";

        var patternCheck = Regex.IsMatch(value, pattern);

        if (!patternCheck)
        {
            return Errors.General.EnglishCharactersOnly(nameof(Identifier));
        }

        return new Identifier(value);
    }
}