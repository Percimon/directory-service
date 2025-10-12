namespace DirectoryService.Domain.Identifiers;

public record DepartmentId
{
    public Guid Value { get; }

    private DepartmentId(Guid value)
    {
        Value = value;
    }

    public static DepartmentId Create(Guid id) => new DepartmentId(id);

    public static DepartmentId New() => new DepartmentId(Guid.NewGuid());

    public static DepartmentId Empty() => new DepartmentId(Guid.Empty);

    public static implicit operator DepartmentId(Guid id) => new(id);
}