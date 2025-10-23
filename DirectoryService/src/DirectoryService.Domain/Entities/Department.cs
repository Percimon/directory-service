using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public class Department : SharedKernel.Entity<DepartmentId>
{
    private List<Department> _children;

    private List<DepartmentPosition> _departmentPositions;

    private List<DepartmentLocation> _departmentLocations;

    private bool _isActive = true;

    //ef core
    private Department(DepartmentId id) : base(id)
    {
    }

    public Department(
        DepartmentId id,
        Name name,
        Identifier identifier,
        Department? parent,
        Path path,
        DepartmentDepth departmentDepth,
        DateTime createdAt,
        DepartmentLocation? location) : base(id)
    {
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = departmentDepth;
        CreatedAt = createdAt;

        _children = [];

        _departmentLocations = location is null ? [] : [location];

        _departmentPositions = [];
    }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public Path Path { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public bool IsActive => _isActive;

    public DepartmentDepth Depth { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public UnitResult<Error> AddChildDepartment(Department child)
    {
        var searchResult = _children.FirstOrDefault(x => x.Id.Value == child.Id.Value);

        if (searchResult is null)
        {
            _children.Add(child);

            return Result.Success<Error>();
        }

        return Errors.General.AlreadyExists(nameof(Department), nameof(Children), child.Id.Value.ToString());
    }

    public UnitResult<Error> RemoveChildDepartment(Department child)
    {
        _children.RemoveAll(x => x.Id.Value == child.Id.Value);

        return Result.Success<Error>();
    }

    public UnitResult<Error> AddLocation(Guid locationId)
    {
        var searchResult = _departmentLocations
            .FirstOrDefault(x => x.LocationId.Value == locationId);

        if (searchResult is null)
        {
            _departmentLocations.Add(DepartmentLocation.Create(Id, LocationId.Create(locationId)).Value);

            return Result.Success<Error>();
        }

        return Errors.General.AlreadyExists(nameof(Department), nameof(locationId), locationId.ToString());
    }

    public UnitResult<Error> RemoveLocation(Guid locationId)
    {
        _departmentLocations.RemoveAll(x => x.LocationId.Value == locationId);

        return Result.Success<Error>();
    }

    public UnitResult<Error> AddPosition(Guid positionId)
    {
        var searchResult = _departmentPositions
            .FirstOrDefault(x => x.PositionId.Value == positionId);

        if (searchResult is null)
        {
            _departmentPositions.Add(DepartmentPosition.Create(Id, PositionId.Create(positionId)).Value);

            return Result.Success<Error>();
        }

        return Errors.General.AlreadyExists(nameof(Department), nameof(positionId), positionId.ToString());
    }

    public UnitResult<Error> RemovePosition(Guid positionId)
    {
        _departmentPositions.RemoveAll(x => x.PositionId.Value == positionId);

        return Result.Success<Error>();
    }
}