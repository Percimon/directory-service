using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentPosition
{
    private DepartmentPosition(DepartmentId departmentId, Guid positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public DepartmentId DepartmentId { get; }

    public Guid PositionId { get; }

    public static Result<DepartmentPosition, Error> Create(DepartmentId departmentId, Guid positionId)
    {
        return new DepartmentPosition(departmentId, positionId);
    }
}