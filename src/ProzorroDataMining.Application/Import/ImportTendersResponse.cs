namespace ProzorroDataMining.Application.Import;

public sealed record ImportTendersResponse(
    ImportDirection Direction,
    int FeedItemsScanned,
    int CandidatesFound,
    int ImportedCount,
    int UpdatedCount,
    int SkippedCount,
    bool IsCompleted,
    string? NextPageUri,
    string? PrevPageUri);