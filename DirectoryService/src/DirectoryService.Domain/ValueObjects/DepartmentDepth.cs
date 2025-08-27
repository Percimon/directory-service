using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentDepth
{
    private DepartmentDepth(short value)
    {
        Value = value;
    }

    public short Value { get; }

    public static Result<DepartmentDepth, Error> Create(short value)
    {
        if (value < 0)
        {
            return Errors.General.ValueIsInvalid(nameof(DepartmentDepth));
        }
        
        return new DepartmentDepth(value);
    }
}