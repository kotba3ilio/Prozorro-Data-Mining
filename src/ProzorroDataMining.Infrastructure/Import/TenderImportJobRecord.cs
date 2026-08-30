using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class TenderImportJobRecord
{
    private TenderImportJobRecord()
    {
    }

    private TenderImportJobRecord(TenderImportJob job)
    {
        Id = job.Id;
        CreatedAt = job.CreatedAt.ToUniversalTime();
        UpdateFromJob(job);
    }

    public Guid Id { get; private set; }

    public TenderImportJobStatus Status { get; private set; }

    public string ClassificationId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedFrom { get; private set; }

    public DateTimeOffset CreatedTo { get; private set; }

    public int MaxPages { get; private set; }

    public int PageSize { get; private set; }

    public ImportDirection Direction { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public ImportDirection? ResultDirection { get; private set; }

    public int? FeedItemsScanned { get; private set; }

    public int? CandidatesFound { get; private set; }

    public int? ImportedCount { get; private set; }

    public int? UpdatedCount { get; private set; }

    public int? SkippedCount { get; private set; }

    public bool? IsCompleted { get; private set; }

    public string? NextPageUri { get; private set; }

    public string? PrevPageUri { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static TenderImportJobRecord Create(TenderImportJob job)
    {
        return new TenderImportJobRecord(job);
    }

    public void MarkInterrupted(DateTimeOffset completedAt)
    {
        Status = TenderImportJobStatus.Failed;
        CompletedAt = completedAt.ToUniversalTime();
        ErrorMessage = "Application restarted before the import job completed.";
    }

    public void UpdateFromJob(TenderImportJob job)
    {
        Status = job.Status;
        ClassificationId = job.Request.ClassificationId;
        CreatedFrom = job.Request.CreatedFrom.ToUniversalTime();
        CreatedTo = job.Request.CreatedTo.ToUniversalTime();
        MaxPages = job.Request.MaxPages;
        PageSize = job.Request.PageSize;
        Direction = job.Request.Direction;
        StartedAt = job.StartedAt?.ToUniversalTime();
        CompletedAt = job.CompletedAt?.ToUniversalTime();
        ErrorMessage = job.ErrorMessage;

        if (job.Result is null)
        {
            ResultDirection = null;
            FeedItemsScanned = null;
            CandidatesFound = null;
            ImportedCount = null;
            UpdatedCount = null;
            SkippedCount = null;
            IsCompleted = null;
            NextPageUri = null;
            PrevPageUri = null;
            return;
        }

        ResultDirection = job.Result.Direction;
        FeedItemsScanned = job.Result.FeedItemsScanned;
        CandidatesFound = job.Result.CandidatesFound;
        ImportedCount = job.Result.ImportedCount;
        UpdatedCount = job.Result.UpdatedCount;
        SkippedCount = job.Result.SkippedCount;
        IsCompleted = job.Result.IsCompleted;
        NextPageUri = job.Result.NextPageUri;
        PrevPageUri = job.Result.PrevPageUri;
    }

    public TenderImportJob ToJob()
    {
        ImportTendersResponse? result = null;

        if (ResultDirection is not null &&
            FeedItemsScanned is not null &&
            CandidatesFound is not null &&
            ImportedCount is not null &&
            UpdatedCount is not null &&
            SkippedCount is not null &&
            IsCompleted is not null)
        {
            result = new ImportTendersResponse(
                ResultDirection.Value,
                FeedItemsScanned.Value,
                CandidatesFound.Value,
                ImportedCount.Value,
                UpdatedCount.Value,
                SkippedCount.Value,
                IsCompleted.Value,
                NextPageUri,
                PrevPageUri);
        }

        return new TenderImportJob(
            Id,
            new ImportTendersRequest(
                ClassificationId,
                CreatedFrom,
                CreatedTo,
                MaxPages,
                PageSize,
                Direction),
            Status,
            CreatedAt,
            StartedAt,
            CompletedAt,
            result,
            ErrorMessage);
    }
}
