namespace DirectoryService.Contracts.Requests;

public record UpdateLocationsRequest(IReadOnlyList<Guid> LocationIds);