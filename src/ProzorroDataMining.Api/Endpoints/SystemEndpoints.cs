using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Infrastructure;

namespace ProzorroDataMining.Api.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystem(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }))
            .WithName("Health")
            .WithTags("System")
            .WithOpenApi();

        endpointRouteBuilder.MapGet("/api/health/ready", async (
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                return canConnect
                    ? Results.Ok(new { status = "Ready" })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("Readiness")
            .WithTags("System")
            .WithOpenApi();

        return endpointRouteBuilder;
    }
}
