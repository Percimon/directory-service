using CSharpFunctionalExtensions;
using DirectoryService.Domain.Abstractions;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedService.SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public sealed class Department : SharedService.SharedKernel.Entity<DepartmentId>, ISoftDeletable
{
    private List<Department> _children = [];

    private List<DepartmentPosition> _departmentPositions = [];

    private List<DepartmentLocation> _departmentLocations = [];

    private bool _isActive = true;

    //ef core
    private Department(DepartmentId id)
        : base(id)
    {
    }

    private Department(
        DepartmentId id,
        Name name,
        Slug slug,
        Department? parent,
        Path path,
        DepartmentDepth departmentDepth,
        IEnumerable<DepartmentLocation> locations)
        : base(id)
    {
        Name = name;
        Slug = slug;
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

    public Slug Slug { get; private set; } = null!;

    public Department? Parent { get; private set; }

    public Path Path { get; private set; } = null!;

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public bool IsActive => _isActive;

    public DepartmentDepth Depth { get; private set; } = null!;

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static Result<Department, Error> CreateParent(
        Name name,
        Slug identifier,
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
        Slug identifier,
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
        var search = _departmentLocations.FirstOrDefault(x => x.LocationId.Value == locationId);

        if (search is not null)
            return GeneralErrors.AlreadyExists(nameof(Location), nameof(DepartmentLocations), locationId.ToString());

        _departmentLocations.Add(DepartmentLocation.Create(Id, LocationId.Create(locationId)).Value);

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateLocations(IEnumerable<Guid> locationIds)
    {
        var departmentLocations = locationIds
            .Select(i => DepartmentLocation.Create(this.Id, LocationId.Create(i)).Value)
            .ToList();

        _departmentLocations = departmentLocations;

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveLocation(Guid locationId)
    {
        var search = _departmentLocations.FirstOrDefault(x => x.LocationId.Value == locationId);

        if (search is null)
            return GeneralErrors.NotFound(locationId);

        _departmentLocations.RemoveAll(x => x.LocationId.Value == locationId);

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddPosition(Guid positionId)
    {
        var searchResult = _departmentPositions
            .FirstOrDefault(x => x.PositionId.Value == positionId);

        if (searchResult is null)
        {
            _departmentPositions.Add(DepartmentPosition.Create(Id, PositionId.Create(positionId)).Value);

            return UnitResult.Success<Error>();
        }

        UpdatedAt = DateTime.UtcNow;

        return GeneralErrors.AlreadyExists(nameof(Position), nameof(positionId), positionId.ToString());
    }

    public UnitResult<Error> RemovePosition(Guid positionId)
    {
        var search = _departmentPositions.FirstOrDefault(x => x.PositionId.Value == positionId);

        if (search is null)
            return GeneralErrors.NotFound(positionId);

        _departmentPositions.RemoveAll(x => x.PositionId.Value == positionId);

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateMainInfo(Name name, Slug slug)
    {
        Name = name;
        Slug = slug;

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SoftDelete()
    {
        _isActive = false;

        DeletedAt = DateTime.UtcNow;

        if (Children.Count > 0)
        {
            foreach (var child in Children)
            {
                child.SoftDelete();
            }
        }

        return UnitResult.Success<Error>();
    }
}