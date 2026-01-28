using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Entities;

public sealed class DepartmentLocation
{
    //ef core
    private DepartmentLocation()
    {

    }

    private DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; }

    public Department Department { get; }

    public DepartmentId DepartmentId { get; }

    public LocationId LocationId { get; }

    public static Result<DepartmentLocation, Error> Create(DepartmentId departmentId, LocationId locationId)
    {
        return new DepartmentLocation(departmentId, locationId);
    }
}