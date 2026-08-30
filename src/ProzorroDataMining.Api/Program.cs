var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.MapSystem();


app.MapAnalytics();
app.MapImport();
app.MapTenders();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

await ApplyDatabaseMigrationsAsync(app);

app.Run();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigrations");

    var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

    if (pendingMigrations.Count == 0)
    {
        logger.LogInformation("Database is up to date. No migrations were applied.");
        return;
    }

    logger.LogInformation(
        "Applying {MigrationCount} database migrations: {Migrations}",
        pendingMigrations.Count,
        string.Join(", ", pendingMigrations));

    await dbContext.Database.MigrateAsync();

    logger.LogInformation("Database migrations applied successfully.");
}