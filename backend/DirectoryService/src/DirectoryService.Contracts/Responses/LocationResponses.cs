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

public record GetLocationTopResponse
{
    public Guid LocationId { get; init; }
    public string City { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string Structure { get; init; } = string.Empty;
    public int DepartmentsCount { get; init; }
};