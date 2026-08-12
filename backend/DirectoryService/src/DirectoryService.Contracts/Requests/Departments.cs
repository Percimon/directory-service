namespace DirectoryService.Contracts.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Slug,
    Guid? ParentId,
    IReadOnlyList<Guid> Locations);

public record UpdateDepartmentRequest(
    string Name,
    string Slug,
    Guid? ParentId);

public record ChangeParentRequest(Guid? NewParentId);

public record UpdateLocationsRequest(IReadOnlyList<Guid> LocationIds);