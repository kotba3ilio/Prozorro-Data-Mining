namespace ProzorroDataMining.Infrastructure.Prozorro;

public sealed class ProzorroRateLimitHandler(ProzorroRequestThrottler requestThrottler) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await requestThrottler.WaitAsync(cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
