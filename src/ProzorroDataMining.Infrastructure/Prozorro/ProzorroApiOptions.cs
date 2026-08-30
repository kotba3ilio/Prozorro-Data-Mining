namespace ProzorroDataMining.Infrastructure.Prozorro;

public sealed class ProzorroApiOptions
{
    public const string SectionName = "ProzorroApi";

    public string BaseAddress { get; init; } = "https://public-api.prozorro.gov.ua/api/2.5/";

    public int RequestTimeoutSeconds { get; init; } = 30;

    public int DefaultPageSize { get; init; } = 500;

    public int DefaultMaxPages { get; init; } = 20;

    public int MaxConcurrentDetailRequests { get; init; } = 8;

    public int MaxRequestsPerSecond { get; init; } = 100;

    public int TooManyRequestsMaxRetries { get; init; } = 3;

    public int TooManyRequestsRetryDelayMs { get; init; } = 1000;
}