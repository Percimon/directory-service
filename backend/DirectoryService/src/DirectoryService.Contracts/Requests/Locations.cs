namespace DirectoryService.Contracts.Requests;

public record LocationDto(
    Guid Id,
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateLocationRequest(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);

public record UpdateLocationRequest(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);