namespace ProzorroDataMining.Application.Analytics;

public static class AnalyticsDefaults
{
    public const string ElectricityClassificationId = "09310000-5";

    public static readonly DateTimeOffset December2025From =
        new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.FromHours(2)).ToUniversalTime();

    public static readonly DateTimeOffset December2025To =
        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(2)).ToUniversalTime();

    public const int TopLimit = 5;
}
