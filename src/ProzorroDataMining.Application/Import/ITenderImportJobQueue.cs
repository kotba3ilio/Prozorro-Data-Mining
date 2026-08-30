namespace ProzorroDataMining.Application.Import;

public interface ITenderImportJobQueue
{
    ValueTask<TenderImportJob> EnqueueAsync(
        ImportTendersRequest request,
        CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(
        TenderImportJob job,
        CancellationToken cancellationToken = default);

    TenderImportJob? GetById(Guid jobId);

    IReadOnlyList<TenderImportJob> GetJobs();

    IReadOnlyList<TenderImportJob> GetRecentJobs(int limit);
}
