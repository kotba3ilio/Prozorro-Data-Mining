namespace ProzorroDataMining.Application.Analytics;

public sealed record AnalyticsFilter(
    string ClassificationId,
    DateTimeOffset CreatedFrom,
    DateTimeOffset CreatedTo,
    int Limit)
{
    public static AnalyticsFilter Default => new(
        AnalyticsDefaults.ElectricityClassificationId,
        AnalyticsDefaults.December2025From,
        AnalyticsDefaults.December2025To,
        AnalyticsDefaults.TopLimit);

    public AnalyticsFilter WithLimit(int limit)
    {
        return this with { Limit = Math.Max(1, limit) };
    }
}