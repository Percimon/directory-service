namespace DirectoryService.Contracts.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    IReadOnlyList<Guid> Locations);