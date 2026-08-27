namespace DirectoryService.Domain.Abstractions;

public interface ISoftDeletable
{
    public bool IsActive { get; }

    public DateTime? DeletedAt { get; }
}