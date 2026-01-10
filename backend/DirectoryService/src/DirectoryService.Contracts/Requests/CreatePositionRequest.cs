namespace DirectoryService.Contracts.Requests;

public record CreatePositionRequest(
    string Name,
    string Description,
    IReadOnlyList<Guid> Departments);