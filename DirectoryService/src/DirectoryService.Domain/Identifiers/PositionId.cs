namespace DirectoryService.Domain.Identifiers;

public record PositionId
{
    public Guid Value { get; }

    private PositionId(Guid value)
    {
        Value = value;
    }

    public static PositionId Create(Guid id) => new PositionId(id);

    public static PositionId New() => new PositionId(Guid.NewGuid());

    public static PositionId Empty() => new PositionId(Guid.Empty);
}