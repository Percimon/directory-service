namespace DirectoryService.Contracts.Dtos;

public record GetDepartmentResponse(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    Guid? ParentId,
    DateTime CreatedAt,
    DateTime UpdatedAt);