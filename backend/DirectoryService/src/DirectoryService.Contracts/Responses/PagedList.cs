namespace DirectoryService.Contracts.Responses;

public sealed class PagedList<T>
{
    public int PageSize { get; init; }

    public int Page { get; init; }

    public long TotalCount { get; init; }

    public IReadOnlyList<T> Items { get; init; } = [];

    public bool HasNextPage => TotalCount > Page * PageSize;

    public bool HasPreviousPage => Page > 1;
}