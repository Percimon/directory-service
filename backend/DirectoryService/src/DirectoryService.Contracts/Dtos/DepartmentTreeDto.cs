namespace DirectoryService.Contracts.Dtos;

public record DepartmentTreeDto(
    Guid Id,
    string Name,
    string Slug,
    string Path,
    int Depth,
    bool HasChildren,
    int ChildrenCount);