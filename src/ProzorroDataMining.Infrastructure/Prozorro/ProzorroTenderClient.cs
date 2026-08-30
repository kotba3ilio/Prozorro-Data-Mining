using System.Net.Http.Json;
using System.Text.Json;

namespace ProzorroDataMining.Infrastructure.Prozorro;

public sealed class ProzorroTenderClient(HttpClient httpClient) : IProzorroTenderClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<ProzorroTenderFeedResponse> GetTenderFeedAsync(
        string? pageUri,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var requestUri = string.IsNullOrWhiteSpace(pageUri)
            ? string.Concat("tenders?descending=1&limit=", pageSize, "&opt_fields=status,dateCreated,public_modified")
            : pageUri;

        using var response = await SendGetAsync(requestUri, cancellationToken);
        var feedResponse = await response.Content.ReadFromJsonAsync<ProzorroTenderFeedResponse>(
            JsonSerializerOptions,
            cancellationToken);

        return feedResponse ?? new ProzorroTenderFeedResponse();
    }

    public async Task<string> GetTenderDetailsJsonAsync(
        string tenderId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendGetAsync(
            string.Concat("tenders/", tenderId),
            cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendGetAsync(
        string requestUri,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }
}