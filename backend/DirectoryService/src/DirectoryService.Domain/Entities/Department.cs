using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public sealed class Department : SharedKernel.Entity<DepartmentId>
{
    private readonly List<Department> _children;

    private readonly List<DepartmentPosition> _departmentPositions;

    private readonly List<DepartmentLocation> _departmentLocations;

    private bool _isActive = true;

    //ef core
    private Department(DepartmentId id) : base(id)
    {
    }

    private Department(
        DepartmentId id,
        Name name,
        Identifier identifier,
        Department? parent,
        Path path,
        DepartmentDepth departmentDepth,
        IEnumerable<DepartmentLocation> locations) : base(id)
    {
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = departmentDepth;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        _children = [];

        _departmentLocations = locations.ToList();

        _departmentPositions = [];
    }

    public Name Name { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public Department? Parent { get; private set; }

    public Path Path { get; private set; } = null!;

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public bool IsActive => _isActive;

    public DepartmentDepth Depth { get; private set; } = null!;

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department, Error> CreateParent(
        Name name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? id = null)
    {
        var locations = departmentLocations.ToList();

        if (locations.Count == 0)
        {
            return Error.Validation("department.location", "Department locations should contain at least one location");
        }

        var path = Path.CreateParent(identifier);
        if (path.IsFailure)
        {
            return path.Error;
        }

        var departmentDepth = DepartmentDepth.Create(0).Value;

        return new Department(
            id ?? DepartmentId.Create(Guid.NewGuid()),
            name,
            identifier,
            null,
            path.Value,
            departmentDepth,
            locations);
    }

    public static Result<Department, Error> CreateChild(
        Name name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? id = null)
    {
        if (parent is null)
        {
            return Error.Validation("department.parent", "Child shoud have parent");
        }

        var path = parent.Path.CreateChild(identifier);
        if (path.IsFailure)
        {
            return path.Error;
        }

        var locations = departmentLocations.ToList();

        if (locations.Count == 0)
        {
            return Error.Validation("department.location", "Department locations should contain at least one location");
        }

        var departmentDepth = DepartmentDepth.Create(parent.Depth.Value + 1).Value;

        return new Department(
            id ?? DepartmentId.Create(Guid.NewGuid()),
            name,
            identifier,
            parent,
            path.Value,
            departmentDepth,
            locations);
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

        return GeneralErrors.AlreadyExists(nameof(Department), nameof(locationId), locationId.ToString());
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

        return GeneralErrors.AlreadyExists(nameof(Department), nameof(positionId), positionId.ToString());
    }

    public UnitResult<Error> RemovePosition(Guid positionId)
    {
        _departmentPositions.RemoveAll(x => x.PositionId.Value == positionId);

        return Result.Success<Error>();
    }
}