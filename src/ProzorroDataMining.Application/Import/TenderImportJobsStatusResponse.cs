namespace ProzorroDataMining.Application.Import;

public sealed record TenderImportJobsStatusResponse(
    bool HasActiveJobs,
    int QueuedCount,
    int RunningCount,
    TenderImportJobResponse? ActiveJob,
    IReadOnlyList<TenderImportJobResponse> RecentJobs);
