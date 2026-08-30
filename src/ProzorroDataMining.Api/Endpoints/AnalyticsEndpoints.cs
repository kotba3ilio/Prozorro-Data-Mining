using ProzorroDataMining.Application.Analytics;

namespace ProzorroDataMining.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalytics(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .WithOpenApi();

        group.MapGet("/summary", async (
                IAnalyticsService analyticsService,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? classificationId,
                int? limit,
                CancellationToken cancellationToken) =>
            {
                var filter = CreateFilter(createdFrom, createdTo, classificationId, limit);
                var response = await analyticsService.GetSummaryAsync(filter, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("GetAnalyticsSummary");

        group.MapGet("/savings", async (
                IAnalyticsService analyticsService,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? classificationId,
                CancellationToken cancellationToken) =>
            {
                var filter = CreateFilter(createdFrom, createdTo, classificationId, AnalyticsDefaults.TopLimit);
                var totalSavings = await analyticsService.GetTotalSavingsAsync(filter, cancellationToken);

                return Results.Ok(new { totalSavings });
            })
            .WithName("GetTotalSavings");

        group.MapGet("/top-procuring-entities", async (
                IAnalyticsService analyticsService,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? classificationId,
                int? limit,
                CancellationToken cancellationToken) =>
            {
                var filter = CreateFilter(createdFrom, createdTo, classificationId, limit);
                var response = await analyticsService.GetTopProcuringEntitiesAsync(filter, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("GetTopProcuringEntities");

        group.MapGet("/top-suppliers", async (
                IAnalyticsService analyticsService,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? classificationId,
                int? limit,
                CancellationToken cancellationToken) =>
            {
                var filter = CreateFilter(createdFrom, createdTo, classificationId, limit);
                var response = await analyticsService.GetTopSuppliersAsync(filter, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("GetTopSuppliers");

        return endpointRouteBuilder;
    }

    private static AnalyticsFilter CreateFilter(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        string? classificationId,
        int? limit)
    {
        return new AnalyticsFilter(
            string.IsNullOrWhiteSpace(classificationId)
                ? AnalyticsDefaults.ElectricityClassificationId
                : classificationId,
            createdFrom ?? AnalyticsDefaults.December2025From,
            createdTo ?? AnalyticsDefaults.December2025To,
            limit.GetValueOrDefault(AnalyticsDefaults.TopLimit))
            .WithLimit(limit.GetValueOrDefault(AnalyticsDefaults.TopLimit));
    }
}