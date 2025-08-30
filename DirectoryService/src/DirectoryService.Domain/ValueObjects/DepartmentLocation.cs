using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentLocation
{
    private DepartmentLocation(DepartmentId departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public DepartmentId DepartmentId { get; }
    
    public Guid LocationId { get; }
    
    public static Result<DepartmentLocation, Error> Create(DepartmentId departmentId, Guid locationId)
    {
        return new DepartmentLocation(departmentId, locationId);
    }
}