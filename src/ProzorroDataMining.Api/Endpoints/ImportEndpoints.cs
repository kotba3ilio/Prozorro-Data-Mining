using ProzorroDataMining.Application.Analytics;
using ProzorroDataMining.Application.Import;

namespace ProzorroDataMining.Api.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImport(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/api/import")
            .WithTags("Import")
            .WithOpenApi();

        group.MapPost("/tenders", async (
                ITenderImportJobQueue tenderImportJobQueue,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? classificationId,
                int? maxPages,
                int? pageSize,
                ImportDirection? direction,
                CancellationToken cancellationToken) =>
            {
                var request = new ImportTendersRequest(
                    string.IsNullOrWhiteSpace(classificationId)
                        ? AnalyticsDefaults.ElectricityClassificationId
                        : classificationId,
                    createdFrom ?? AnalyticsDefaults.December2025From,
                    createdTo ?? AnalyticsDefaults.December2025To,
                    Math.Max(1, maxPages.GetValueOrDefault(20)),
                    Math.Clamp(pageSize.GetValueOrDefault(500), 1, 1000),
                    direction ?? ImportDirection.Backward);

                var job = await tenderImportJobQueue.EnqueueAsync(request, cancellationToken);
                var response = TenderImportJobResponse.FromJob(job);

                return Results.Accepted($"/api/import/tenders/jobs/{job.Id}", response);
            })
            .WithName("CreateTenderImportJob");

        group.MapGet("/tenders/jobs/status", (
                ITenderImportJobQueue tenderImportJobQueue,
                int? limit) =>
            {
                var jobs = tenderImportJobQueue
                    .GetRecentJobs(Math.Clamp(limit.GetValueOrDefault(10), 1, 50))
                    .Select(TenderImportJobResponse.FromJob)
                    .ToList();
                var queuedCount = jobs.Count(job => job.Status == TenderImportJobStatus.Queued);
                var runningCount = jobs.Count(job => job.Status == TenderImportJobStatus.Running);
                var activeJob = jobs.FirstOrDefault(job =>
                    job.Status is TenderImportJobStatus.Running or TenderImportJobStatus.Queued);

                return Results.Ok(new TenderImportJobsStatusResponse(
                    queuedCount > 0 || runningCount > 0,
                    queuedCount,
                    runningCount,
                    activeJob,
                    jobs));
            })
            .WithName("GetTenderImportJobsStatus");

        group.MapGet("/tenders/jobs/{jobId:guid}", (
                ITenderImportJobQueue tenderImportJobQueue,
                Guid jobId) =>
            {
                var job = tenderImportJobQueue.GetById(jobId);

                return job is null
                    ? Results.NotFound()
                    : Results.Ok(TenderImportJobResponse.FromJob(job));
            })
            .WithName("GetTenderImportJob");

        return endpointRouteBuilder;
    }
}
