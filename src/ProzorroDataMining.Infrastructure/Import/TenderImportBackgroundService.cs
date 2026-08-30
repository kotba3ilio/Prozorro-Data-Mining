using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Infrastructure.Import;

public sealed class TenderImportBackgroundService(
    TenderImportBackgroundJobQueue jobQueue,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TenderImportBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var requeuedJobCount = await jobQueue.RequeueUnfinishedJobsAsync(stoppingToken);

        if (requeuedJobCount > 0)
        {
            logger.LogInformation("Requeued {JobCount} unfinished tender import jobs after application restart.", requeuedJobCount);
        }

        await foreach (var job in jobQueue.ReadAllAsync(stoppingToken))
        {
            await ProcessJobAsync(job, stoppingToken);
        }
    }

    private async Task ProcessJobAsync(
        TenderImportJob job,
        CancellationToken stoppingToken)
    {
        job.MarkRunning(DateTimeOffset.UtcNow);
        await jobQueue.UpdateAsync(job, stoppingToken);

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<ITenderImportService>();
            var result = await importService.ImportAsync(job.Request, stoppingToken);

            job.MarkCompleted(result, DateTimeOffset.UtcNow);
            await jobQueue.UpdateAsync(job, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            job.MarkQueued("Application is stopping. The job will be retried after restart.");
            await jobQueue.UpdateAsync(job, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Tender import background job {JobId} failed.",
                job.Id);

            job.MarkFailed(exception.Message, DateTimeOffset.UtcNow);
            await jobQueue.UpdateAsync(job, CancellationToken.None);
        }
    }
}
