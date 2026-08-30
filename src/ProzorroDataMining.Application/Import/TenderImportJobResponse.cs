namespace ProzorroDataMining.Application.Import;

public sealed record TenderImportJobResponse(
    Guid JobId,
    TenderImportJobStatus Status,
    ImportDirection RequestDirection,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    ImportTendersResponse? Result,
    string? ErrorMessage)
{
    public static TenderImportJobResponse FromJob(TenderImportJob job)
    {
        return new TenderImportJobResponse(
            job.Id,
            job.Status,
            job.Request.Direction,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.Result,
            job.ErrorMessage);
    }
}
