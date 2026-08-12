namespace DirectoryService.Contracts.Requests;

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