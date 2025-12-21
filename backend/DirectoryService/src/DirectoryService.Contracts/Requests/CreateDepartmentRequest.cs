namespace DirectoryService.Contracts.Requests;

public record CreateDepartmentRequest(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);