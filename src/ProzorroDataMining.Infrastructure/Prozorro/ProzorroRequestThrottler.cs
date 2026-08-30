using Microsoft.Extensions.Options;

namespace ProzorroDataMining.Infrastructure.Prozorro;

public sealed class ProzorroRequestThrottler(IOptions<ProzorroApiOptions> options)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTimeOffset _nextAllowedRequestAt = DateTimeOffset.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var requestsPerSecond = Math.Clamp(options.Value.MaxRequestsPerSecond, 1, 140);
            var delayBetweenRequests = TimeSpan.FromMilliseconds(
                Math.Ceiling(1000d / requestsPerSecond));
            var now = DateTimeOffset.UtcNow;

            if (_nextAllowedRequestAt > now)
            {
                await Task.Delay(_nextAllowedRequestAt - now, cancellationToken);
                now = DateTimeOffset.UtcNow;
            }

            _nextAllowedRequestAt = now + delayBetweenRequests;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
