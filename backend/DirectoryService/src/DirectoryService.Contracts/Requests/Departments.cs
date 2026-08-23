namespace DirectoryService.Contracts.Requests;

public record GetDepartmentsRequest(
    int Page = 1,
    int PageSize = 20,
    string SortBy = "name",
    string SortDirection = "asc",
    string? Search = "");

public record CreateDepartmentRequest(
    string Name,
    string Slug,
    Guid? ParentId,
    IReadOnlyList<Guid> Locations);

public record UpdateDepartmentRequest(
    string Name,
    string Slug);

public record ChangeParentRequest(Guid? NewParentId);

public record UpdateLocationsRequest(IReadOnlyList<Guid> LocationIds);