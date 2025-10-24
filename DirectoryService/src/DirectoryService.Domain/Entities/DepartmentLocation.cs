using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using SharedKernel;

namespace DirectoryService.Domain.Entities;

public record DepartmentLocation
{
    //ef croe
    private DepartmentLocation()
    {

    }

    private DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; }

    public Department Department { get; } = null!;

    public DepartmentId DepartmentId { get; }

    public LocationId LocationId { get; }

    public static Result<DepartmentLocation, Error> Create(DepartmentId departmentId, LocationId locationId)
    {
        return new DepartmentLocation(departmentId, locationId);
    }
}