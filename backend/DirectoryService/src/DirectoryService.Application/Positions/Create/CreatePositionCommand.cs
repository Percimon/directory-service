namespace DirectoryService.Application.Positions.Create;

public record CreatePositionCommand(
    string Name,
    string Description);
