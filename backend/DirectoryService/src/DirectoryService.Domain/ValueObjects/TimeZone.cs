using CSharpFunctionalExtensions;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record TimeZone
{
    private TimeZone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<TimeZone, Error> Create(string value)
    {
        var result = TimeZoneInfo.TryFindSystemTimeZoneById(value, out _);

        if (!result)
        {
            return GeneralErrors.ValueIsInvalid(nameof(TimeZone));
        }

        return new TimeZone(value);
    }
}