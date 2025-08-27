using CSharpFunctionalExtensions;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public class Department : SharedKernel.Entity<DepartmentId>
{
    private List<Position> _positions;
    
    private List<Location> _locations;
    
    private List<Department> _children;

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
        
        _locations = location is null ? [] : [location];
        
        _positions = [];
    }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public DepartmentId? ParentId { get; private set;}

    public Path Path { get; private set; }

    public IReadOnlyList<Department> Children => _children;
    
    public IReadOnlyList<Location> Locations => _locations;
    
    public IReadOnlyList<Position> Positions => _positions;
    
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
    
    public UnitResult<Error> AddLocation(Location location)
    {
        var searchResult = _locations
            .FirstOrDefault(x => x.Id.Value == location.Id.Value
                                 || x.Name.Value == location.Name.Value);
        if (searchResult is null)
        {
            _locations.Add(location);
        
            return Result.Success<Error>();
        }
       
        return Errors.General.AlreadyExists(nameof(Department), nameof(Locations), location.Id.Value.ToString());
    }
    
    public UnitResult<Error> RemoveLocation(Location location)
    {
        _children.RemoveAll(x => x.Id.Value == location.Id.Value);
        
        return Result.Success<Error>();
    }
    
    public UnitResult<Error> AddPosition(Position position)
    {
        var searchResult = _positions
            .FirstOrDefault(x => x.Id.Value == position.Id.Value 
                                 || x.Name.Value == position.Name.Value);

        if (searchResult is null)
        {
            _positions.Add(position);
        
            return Result.Success<Error>();
        }
       
        return Errors.General.AlreadyExists(nameof(Department), nameof(Positions), position.Id.Value.ToString());
    }
    
    public UnitResult<Error> RemovePosition(Position position)
    {
        _positions.RemoveAll(x => x.Id.Value == position.Id.Value);
        
        return Result.Success<Error>();
    }
}