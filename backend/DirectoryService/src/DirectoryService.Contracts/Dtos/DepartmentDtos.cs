namespace DirectoryService.Contracts.Dtos;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    Guid? ParentId,
    DateTime CreatedAt,
    DateTime UpdatedAt);