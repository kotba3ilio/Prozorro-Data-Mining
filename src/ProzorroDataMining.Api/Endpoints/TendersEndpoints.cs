using ProzorroDataMining.Application.Analytics;
using ProzorroDataMining.Application.Tenders;

namespace ProzorroDataMining.Api.Endpoints;

public static class TendersEndpoints
{
    public static IEndpointRouteBuilder MapTenders(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/api/tenders")
            .WithTags("Tenders")
            .WithOpenApi();

        group.MapGet("/list", async (
                ITenderService tenderService,
                string? classificationId,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                string? cursor,
                int? pageSize,
                CancellationToken cancellationToken) =>
            {
                if (!TenderPageCursor.TryDecode(cursor, out _))
                {
                    return Results.BadRequest(new { message = "Invalid tender page cursor." });
                }

                var currentPageSize = Math.Clamp(pageSize.GetValueOrDefault(20), 1, 100);
                var filter = new AnalyticsFilter(
                    classificationId ?? AnalyticsDefaults.ElectricityClassificationId,
                    createdFrom ?? AnalyticsDefaults.December2025From,
                    createdTo ?? AnalyticsDefaults.December2025To,
                    currentPageSize);
                var tenders = await tenderService.TendersAsync(
                    filter,
                    cursor,
                    currentPageSize,
                    cancellationToken);

                return Results.Ok(tenders);
            })
            .WithName("ListTenders");

        group.MapGet("/{tenderId:guid}", async (
                ITenderService tenderService,
                Guid tenderId,
                CancellationToken cancellationToken) =>
            {
                var tender = await tenderService.GetTenderByIdAsync(tenderId, cancellationToken);

                if (tender is null)
                {
                    return Results.NotFound(new { message = $"Tender with ID '{tenderId}' not found." });
                }

                return Results.Ok(tender);
            })
            .WithName("GetTenderById");

        return endpointRouteBuilder;
    }
}