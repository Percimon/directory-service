namespace DirectoryService.Contracts;

public record CreateLocationRequest(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);