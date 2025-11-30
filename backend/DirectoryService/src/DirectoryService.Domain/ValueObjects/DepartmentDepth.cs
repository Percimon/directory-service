using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentDepth
{
    private DepartmentDepth(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<DepartmentDepth, Error> Create(int value)
    {
        if (value < 0)
        {
            return Errors.General.ValueIsInvalid(nameof(DepartmentDepth));
        }

        return new DepartmentDepth(value);
    }
}