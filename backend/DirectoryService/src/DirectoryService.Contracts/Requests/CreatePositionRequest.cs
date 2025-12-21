namespace DirectoryService.Contracts.Requests;

public record CreatePositionRequest(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);