using ProzorroDataMining.Application.Import;
using ProzorroDataMining.Infrastructure.Import;

namespace ProzorroDataMining.Infrastructure.Tests;

public sealed class TenderImportJobRecordTests
{
    [Fact]
    public void CreateAndToJob_PreserveRequestAndResult()
    {
        var createdAt = new DateTimeOffset(2025, 12, 1, 10, 0, 0, TimeSpan.Zero);
        var startedAt = createdAt.AddMinutes(1);
        var completedAt = createdAt.AddMinutes(2);
        var request = new ImportTendersRequest(
            "09310000-5",
            createdAt,
            createdAt.AddDays(1),
            4,
            250,
            ImportDirection.Forward);
        var result = new ImportTendersResponse(
            ImportDirection.Forward,
            20,
            10,
            7,
            2,
            1,
            false,
            "next",
            "prev");
        var job = new TenderImportJob(request, createdAt);
        job.MarkRunning(startedAt);
        job.MarkCompleted(result, completedAt);

        var roundTripped = TenderImportJobRecord.Create(job).ToJob();

        Assert.Equal(job.Id, roundTripped.Id);
        Assert.Equal(TenderImportJobStatus.Completed, roundTripped.Status);
        Assert.Equal(ImportDirection.Forward, roundTripped.Request.Direction);
        Assert.Equal("09310000-5", roundTripped.Request.ClassificationId);
        Assert.Equal(4, roundTripped.Request.MaxPages);
        Assert.Equal(250, roundTripped.Request.PageSize);
        Assert.NotNull(roundTripped.Result);
        Assert.Equal(20, roundTripped.Result.FeedItemsScanned);
        Assert.Equal("next", roundTripped.Result.NextPageUri);
        Assert.Equal("prev", roundTripped.Result.PrevPageUri);
    }

    [Fact]
    public void MarkInterrupted_FailsQueuedJobWithRestartMessage()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new TenderImportJob(new ImportTendersRequest(
            "09310000-5",
            now,
            now.AddDays(1),
            1,
            100,
            ImportDirection.Backward), now);
        var record = TenderImportJobRecord.Create(job);

        record.MarkInterrupted(now.AddMinutes(5));
        var restoredJob = record.ToJob();

        Assert.Equal(TenderImportJobStatus.Failed, restoredJob.Status);
        Assert.Equal("Application restarted before the import job completed.", restoredJob.ErrorMessage);
        Assert.Equal(now.AddMinutes(5).ToUniversalTime(), restoredJob.CompletedAt);
    }
}