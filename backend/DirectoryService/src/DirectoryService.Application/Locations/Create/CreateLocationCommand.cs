namespace DirectoryService.Application.Locations.Create;

public record CreateLocationCommand(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);
