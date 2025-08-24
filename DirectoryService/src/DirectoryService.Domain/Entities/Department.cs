using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedKernel;
using Path = DirectoryService.Domain.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public class Department : Entity<DepartmentId>
{
    private bool _isActive = true;

    public Department(DepartmentId id) : base(id)
    {
    }

    public Department(
        DepartmentId id,
        Name name,
        Identifier identifier,
        Department? parent,
        Path path,
        DepartmentDepth departmentDepth,
        DateTime createdAt) : base(id)
    {
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = departmentDepth;
        CreatedAt = createdAt;
    }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }
    
    public DepartmentId? ParentId => Parent?.Id;

    public Path Path { get; private set; }

    public bool IsActive => _isActive;
    
    public DepartmentDepth Depth { get; private set; }
    
    public DateTime CreatedAt { get; }
    
    public DateTime UpdatedAt { get; private set;}
}