namespace DirectoryService.Contracts.Responses;

public record GetLocationResponse(
    Guid Id,
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetLocationTopResponse(
    Guid Id,
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    int DepartmentCount);