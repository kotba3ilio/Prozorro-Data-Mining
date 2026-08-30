namespace ProzorroDataMining.Application.Import;

public sealed class TenderImportJob
{
    public TenderImportJob(ImportTendersRequest request, DateTimeOffset createdAt)
        : this(
            Guid.NewGuid(),
            request,
            TenderImportJobStatus.Queued,
            createdAt,
            null,
            null,
            null,
            null)
    {
    }

    public TenderImportJob(
        Guid id,
        ImportTendersRequest request,
        TenderImportJobStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        ImportTendersResponse? result,
        string? errorMessage)
    {
        Id = id;
        Request = request;
        Status = status;
        CreatedAt = createdAt;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Result = result;
        ErrorMessage = errorMessage;
    }

    public Guid Id { get; }

    public ImportTendersRequest Request { get; }

    public TenderImportJobStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public ImportTendersResponse? Result { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void MarkRunning(DateTimeOffset startedAt)
    {
        Status = TenderImportJobStatus.Running;
        StartedAt = startedAt;
        CompletedAt = null;
        ErrorMessage = null;
    }

    public void MarkCompleted(ImportTendersResponse result, DateTimeOffset completedAt)
    {
        Status = TenderImportJobStatus.Completed;
        Result = result;
        CompletedAt = completedAt;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset completedAt)
    {
        Status = TenderImportJobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = completedAt;
    }

    public void MarkQueued(string? message = null)
    {
        Status = TenderImportJobStatus.Queued;
        StartedAt = null;
        CompletedAt = null;
        Result = null;
        ErrorMessage = message;
    }
}
