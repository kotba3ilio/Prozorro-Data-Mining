namespace ProzorroDataMining.Api.Middleware;

public static class DatabaseMigrationMiddlewareExtensions
{
    public static IApplicationBuilder UseDatabaseMigrations(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DatabaseMigrationMiddleware>();
    }
}