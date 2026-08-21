namespace DirectoryService.Contracts.Dtos;

public record LocationDto(
    Guid Id,
    string Name,
    string City,
    string District,
    string Street,
    string Structure,
    string TimeZone,
    DateTime CreatedAt,
    DateTime UpdatedAt);