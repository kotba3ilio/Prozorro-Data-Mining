namespace ProzorroDataMining.Infrastructure.Prozorro;

public interface IProzorroTenderClient
{
    Task<ProzorroTenderFeedResponse> GetTenderFeedAsync(
        string? pageUri,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<string> GetTenderDetailsJsonAsync(
        string tenderId,
        CancellationToken cancellationToken = default);
}