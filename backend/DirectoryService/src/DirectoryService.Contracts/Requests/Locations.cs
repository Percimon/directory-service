namespace DirectoryService.Contracts.Requests;

public record GetLocationsRequest(
    int Page = 1,
    int PageSize = 20,
    string SortBy = "name",
    string SortDirection = "asc",
    int MinDepartmentsCount = 0,
    string? Search = "");

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