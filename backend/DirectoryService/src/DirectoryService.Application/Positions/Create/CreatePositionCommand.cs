namespace DirectoryService.Application.Positions.Create;

public record CreatePositionCommand(
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone);
