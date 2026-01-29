using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Entities;

public sealed class Position : Entity<PositionId>
{
    private bool _isActive = true;

    //ef core
    private Position(PositionId id) : base(id)
    {
    }

    public Position(
        PositionId id,
        Name name,
        Description description,
        DateTime createdAt) : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public bool IsActive => _isActive;

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }
}