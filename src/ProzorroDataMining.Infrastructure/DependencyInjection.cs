using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using ProzorroDataMining.Application.Abstractions;
using ProzorroDataMining.Application.Import;
using ProzorroDataMining.Infrastructure.Database;
using ProzorroDataMining.Infrastructure.Import;
using ProzorroDataMining.Infrastructure.Prozorro;
using ProzorroDataMining.Infrastructure.Repositories;

namespace ProzorroDataMining.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ProzorroApiOptions>(
            configuration.GetSection(ProzorroApiOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        services.AddSingleton<ProzorroRequestThrottler>();
        services.AddTransient<ProzorroRateLimitHandler>();

        services.AddHttpClient<IProzorroTenderClient, ProzorroTenderClient>((provider, httpClient) =>
        {
            var prozorroOptions = provider.GetRequiredService<IOptions<ProzorroApiOptions>>().Value;

            httpClient.BaseAddress = new Uri(prozorroOptions.BaseAddress);
            httpClient.Timeout = TimeSpan.FromSeconds(prozorroOptions.RequestTimeoutSeconds);
        })
        .AddPolicyHandler((provider, _) => CreateProzorroRetryPolicy(provider))
        .AddHttpMessageHandler<ProzorroRateLimitHandler>();

        services.AddSingleton<TenderImportBackgroundJobQueue>();
        services.AddSingleton<ITenderImportJobQueue>(provider =>
            provider.GetRequiredService<TenderImportBackgroundJobQueue>());
        services.AddHostedService<TenderImportBackgroundService>();

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITenderImportService, TenderImportService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITenderRepository, TenderRepository>();
        services.AddScoped<ITenderItemRepository, TenderItemRepository>();
        services.AddScoped<ITenderContractRepository, TenderContractRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateProzorroRetryPolicy(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<ProzorroApiOptions>>().Value;
        var retryCount = Math.Max(0, options.TooManyRequestsMaxRetries);
        var fallbackDelay = TimeSpan.FromMilliseconds(Math.Max(1, options.TooManyRequestsRetryDelayMs));

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.RequestTimeout)
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .OrResult(response => (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(fallbackDelay.TotalMilliseconds * retryAttempt));
    }
}
