using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Application.Tests;

public sealed class TenderImportJobTests
{
    [Fact]
    public void Lifecycle_TracksStatusTimestampsAndResult()
    {
        var createdAt = new DateTimeOffset(2025, 12, 1, 10, 0, 0, TimeSpan.Zero);
        var startedAt = createdAt.AddMinutes(1);
        var completedAt = createdAt.AddMinutes(3);
        var request = new ImportTendersRequest(
            "09310000-5",
            createdAt,
            createdAt.AddDays(1),
            2,
            100,
            ImportDirection.Backward);
        var result = new ImportTendersResponse(
            ImportDirection.Backward,
            10,
            5,
            3,
            1,
            1,
            true,
            null,
            "prev");

        var job = new TenderImportJob(request, createdAt);
        job.MarkRunning(startedAt);
        job.MarkCompleted(result, completedAt);

        Assert.Equal(TenderImportJobStatus.Completed, job.Status);
        Assert.Equal(createdAt, job.CreatedAt);
        Assert.Equal(startedAt, job.StartedAt);
        Assert.Equal(completedAt, job.CompletedAt);
        Assert.Same(result, job.Result);
        Assert.Null(job.ErrorMessage);
    }

    [Fact]
    public void MarkFailed_StoresErrorAndCompletionTime()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new TenderImportJob(new ImportTendersRequest(
            "09310000-5",
            now,
            now.AddDays(1),
            1,
            100,
            ImportDirection.Forward), now);

        job.MarkFailed("boom", now.AddMinutes(2));

        Assert.Equal(TenderImportJobStatus.Failed, job.Status);
        Assert.Equal("boom", job.ErrorMessage);
        Assert.Equal(now.AddMinutes(2), job.CompletedAt);
    }

    [Fact]
    public void MarkQueued_ReturnsJobToQueueAndClearsCompletionState()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new TenderImportJob(new ImportTendersRequest(
            "09310000-5",
            now,
            now.AddDays(1),
            1,
            100,
            ImportDirection.Forward), now);
        var result = new ImportTendersResponse(
            ImportDirection.Forward,
            1,
            1,
            1,
            0,
            0,
            false,
            "next",
            "prev");

        job.MarkRunning(now.AddMinutes(1));
        job.MarkCompleted(result, now.AddMinutes(2));
        job.MarkQueued("Application is stopping. The job will be retried after restart.");

        Assert.Equal(TenderImportJobStatus.Queued, job.Status);
        Assert.Null(job.StartedAt);
        Assert.Null(job.CompletedAt);
        Assert.Null(job.Result);
        Assert.Equal("Application is stopping. The job will be retried after restart.", job.ErrorMessage);
    }
}
