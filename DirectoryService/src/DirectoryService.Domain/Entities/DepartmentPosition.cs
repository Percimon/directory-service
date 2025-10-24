using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.Entities;

public record DepartmentPosition
{
    //ef core
    private DepartmentPosition()
    {
    }

    private DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public Guid Id { get; }

    public Department Department { get; } = null!;

    public DepartmentId DepartmentId { get; }

    public PositionId PositionId { get; }

    public static Result<DepartmentPosition, Error> Create(DepartmentId departmentId, PositionId positionId)
    {
        return new DepartmentPosition(departmentId, positionId);
    }
}