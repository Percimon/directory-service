using CSharpFunctionalExtensions;
using DirectoryService.Domain.Abstractions;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Entities;

public sealed class Position : SharedService.SharedKernel.Entity<PositionId>, ISoftDeletable
{
    private bool _isActive = true;

    //ef core
    private Position(PositionId id)
        : base(id)
    {
    }

    public Position(
        PositionId id,
        Name name,
        Description description,
        DateTime createdAt)
        : base(id)
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

    public DateTime? DeletedAt { get; private set; }

    public UnitResult<Error> Rename(Name newName)
    {
        Name = newName;

        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SoftDelete()
    {
        _isActive = false;

        DeletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}