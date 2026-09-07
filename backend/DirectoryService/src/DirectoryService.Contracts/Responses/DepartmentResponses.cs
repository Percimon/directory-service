namespace DirectoryService.Contracts.Dtos;

public record GetDepartmentResponse(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    Guid? ParentId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record DepartmentListItemDto(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    DateTime CreatedAt);

public record ChangeParentResponseDto(
    Guid Id,
    Guid? ParentId,
    string Path,
    int Depth,
    DateTime UpdatedAt);