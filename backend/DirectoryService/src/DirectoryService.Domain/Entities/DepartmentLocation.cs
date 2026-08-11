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

    private DepartmentLocation(
        DepartmentId departmentId,
        LocationId locationId,
        bool isPrimary)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        IsPrimary = isPrimary;
    }

    public Guid Id { get; }

    public Department Department { get; }

    public DepartmentId DepartmentId { get; }

    public LocationId LocationId { get; }

    public bool IsPrimary { get; }

    public static Result<DepartmentLocation, Error> Create(
        DepartmentId departmentId,
        LocationId locationId,
        bool isPrimary = false)
    {
        return new DepartmentLocation(departmentId, locationId, isPrimary);
    }
}