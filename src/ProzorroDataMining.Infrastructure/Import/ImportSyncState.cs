using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class ImportSyncState
{
    private ImportSyncState()
    {
    }

    private ImportSyncState(string feedName, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        FeedName = feedName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string FeedName { get; private set; } = string.Empty;

    public string? BackwardNextPageUri { get; private set; }

    public string? ForwardStartPageUri { get; private set; }

    public string? ForwardNextPageUri { get; private set; }

    public ImportDirection LastDirection { get; private set; } = ImportDirection.Backward;

    public decimal? LastPublicModified { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? LastSuccessAt { get; private set; }

    public static ImportSyncState Create(string feedName, DateTimeOffset createdAt)
    {
        return new ImportSyncState(feedName, createdAt);
    }

    public string? GetPageUri(ImportDirection direction)
    {
        return direction == ImportDirection.Backward
            ? BackwardNextPageUri
            : ForwardNextPageUri ?? ForwardStartPageUri;
    }

    public void SaveBackwardCursor(
        string? nextPageUri,
        string? prevPageUri,
        decimal? lastPublicModified,
        DateTimeOffset syncedAt)
    {
        BackwardNextPageUri = string.IsNullOrWhiteSpace(nextPageUri) ? null : nextPageUri;

        if (!string.IsNullOrWhiteSpace(prevPageUri))
        {
            ForwardStartPageUri ??= prevPageUri;
        }

        SaveCommon(ImportDirection.Backward, lastPublicModified, syncedAt);
    }

    public void SaveForwardCursor(
        string? nextPageUri,
        string? prevPageUri,
        decimal? lastPublicModified,
        DateTimeOffset syncedAt)
    {
        ForwardNextPageUri = string.IsNullOrWhiteSpace(nextPageUri) ? ForwardNextPageUri : nextPageUri;

        if (!string.IsNullOrWhiteSpace(prevPageUri))
        {
            ForwardStartPageUri = prevPageUri;
        }

        SaveCommon(ImportDirection.Forward, lastPublicModified, syncedAt);
    }

    private void SaveCommon(
        ImportDirection direction,
        decimal? lastPublicModified,
        DateTimeOffset syncedAt)
    {
        LastDirection = direction;
        LastPublicModified = lastPublicModified ?? LastPublicModified;
        UpdatedAt = syncedAt;
        LastSuccessAt = syncedAt;
    }
}