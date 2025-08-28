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
        Location? location) : base(id)
    {
        Name = name;
        Identifier = identifier;
        Parent = parent;
        ParentId = parent?.Id;
        Path = path;
        Depth = departmentDepth;
        CreatedAt = createdAt;

        _children = [];

        _departmentLocations = location is null ? [] : [DepartmentLocation.Create(id, location.Id.Value).Value];
    }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public DepartmentId? ParentId { get; private set; }

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
            .FirstOrDefault(x => x.LocationId == locationId);
        
        if (searchResult is null)
        {
            _departmentLocations.Add(DepartmentLocation.Create(Id, locationId).Value);

            return Result.Success<Error>();
        }

        return Errors.General.AlreadyExists(nameof(Department), nameof(locationId), locationId.ToString());
    }

    public UnitResult<Error> RemoveLocation(Guid locationId)
    {
        _departmentLocations.RemoveAll(x => x.LocationId == locationId);

        return Result.Success<Error>();
    }

    public UnitResult<Error> AddPosition(Guid positionId)
    {
        var searchResult = _departmentPositions
            .FirstOrDefault(x => x.PositionId == positionId);

        if (searchResult is null)
        {
            _departmentPositions.Add(DepartmentPosition.Create(Id, positionId).Value);

            return Result.Success<Error>();
        }

        return Errors.General.AlreadyExists(nameof(Department), nameof(positionId), positionId.ToString());
    }
    
    public UnitResult<Error> RemovePosition(Guid positionId)
    {
        _departmentPositions.RemoveAll(x => x.PositionId == positionId);

        return Result.Success<Error>();
    }
}