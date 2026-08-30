using Microsoft.EntityFrameworkCore;
using ProzorroDataMining.Infrastructure;

namespace ProzorroDataMining.Api.Middleware;

public sealed class DatabaseMigrationMiddleware
{
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);
    private static bool _migrationsApplied;

    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseMigrationMiddleware> _logger;

    public DatabaseMigrationMiddleware(
        RequestDelegate next,
        ILogger<DatabaseMigrationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        if (!_migrationsApplied)
        {
            await ApplyMigrationsAsync(dbContext);
        }

        await _next(context);
    }

    private async Task ApplyMigrationsAsync(ApplicationDbContext dbContext)
    {
        await MigrationLock.WaitAsync();

        try
        {
            if (_migrationsApplied)
            {
                return;
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Count == 0)
            {
                _logger.LogInformation("Database is up to date. No migrations were applied.");
                _migrationsApplied = true;
                return;
            }

            _logger.LogInformation(
                "Applying {MigrationCount} database migrations: {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            await dbContext.Database.MigrateAsync();

            _migrationsApplied = true;
            _logger.LogInformation("Database migrations applied successfully.");
        }
        finally
        {
            MigrationLock.Release();
        }
    }
}
