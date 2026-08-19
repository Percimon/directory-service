namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentCommand(
    string Name,
    string Slug,
    Guid? ParentId,
    IReadOnlyList<Guid> Locations);