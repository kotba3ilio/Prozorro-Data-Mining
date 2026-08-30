namespace ProzorroDataMining.Application.Tenders;

public sealed record CursorPagedResponse<TItem>(
    IReadOnlyList<TItem> Items,
    int PageSize,
    string? NextCursor,
    bool HasNextPage);