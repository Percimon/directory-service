namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentCommand(
    string Name,
    string Identifier,
    Guid? ParentId,
    IReadOnlyList<Guid> Locations);