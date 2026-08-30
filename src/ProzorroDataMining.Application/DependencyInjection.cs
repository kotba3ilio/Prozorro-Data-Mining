using Microsoft.Extensions.DependencyInjection;
using ProzorroDataMining.Application.Analytics;
using ProzorroDataMining.Application.Tenders;

namespace ProzorroDataMining.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ITenderService, TenderService>();

        return services;
    }
}
