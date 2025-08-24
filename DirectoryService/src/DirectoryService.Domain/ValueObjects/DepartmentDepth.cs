using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentDepth
{
    private DepartmentDepth(short value)
    {
        Value = value;
    }

    public short Value { get; }

    public static Result<DepartmentDepth> Create(short value)
    {
        return new DepartmentDepth(value);
    }
}