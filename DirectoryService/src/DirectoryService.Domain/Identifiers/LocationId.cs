namespace DirectoryService.Domain.Identifiers;

public record LocationId
{
    public Guid Value { get; }

    private LocationId(Guid value)
    {
        Value = value;
    }

    public static LocationId Create(Guid id) => new LocationId(id);

    public static LocationId New() => new LocationId(Guid.NewGuid());

    public static LocationId Empty() => new LocationId(Guid.Empty);
}