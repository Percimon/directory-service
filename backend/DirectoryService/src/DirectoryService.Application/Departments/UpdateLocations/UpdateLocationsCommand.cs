namespace DirectoryService.Application.Departments.UpdateLocations;

public record UpdateLocationsCommand(Guid DepartmentId, IReadOnlyList<Guid> LocationIds);