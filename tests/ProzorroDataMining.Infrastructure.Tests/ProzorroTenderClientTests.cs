using System.Net;
using ProzorroDataMining.Infrastructure.Prozorro;

namespace ProzorroDataMining.Infrastructure.Tests;

public sealed class ProzorroTenderClientTests
{
    [Fact]
    public async Task GetTenderFeedAsync_WhenPageUriIsEmpty_StartsFromDescendingFeed()
    {
        var handler = new CaptureRequestHandler("{\"data\":[]}");
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://public-api.prozorro.gov.ua/api/2.5/")
        };
        var client = new ProzorroTenderClient(httpClient);

        await client.GetTenderFeedAsync(null, pageSize: 500);

        Assert.Equal(
            "https://public-api.prozorro.gov.ua/api/2.5/tenders?descending=1&limit=500&opt_fields=status,dateCreated,public_modified",
            handler.RequestUri?.ToString());
    }

    private sealed class CaptureRequestHandler(string content) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}