namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentCommand(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);