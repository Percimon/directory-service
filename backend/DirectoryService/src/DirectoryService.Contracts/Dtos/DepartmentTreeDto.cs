namespace DirectoryService.Contracts.Dtos;

public record DepartmentTreeDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string Path,
    int Depth,
    bool HasChildren,
    int ChildrenCount);