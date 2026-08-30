using System.Text.Json.Serialization;

namespace ProzorroDataMining.Infrastructure.Prozorro;

public sealed class ProzorroTenderFeedResponse
{
    public IReadOnlyList<ProzorroTenderFeedItem> Data { get; init; } = [];

    [JsonPropertyName("next_page")]
    public ProzorroPageLink? NextPage { get; init; }

    [JsonPropertyName("prev_page")]
    public ProzorroPageLink? PrevPage { get; init; }
}

public sealed class ProzorroTenderFeedItem
{
    public string Id { get; init; } = string.Empty;

    public DateTimeOffset DateModified { get; init; }

    public DateTimeOffset? DateCreated { get; init; }

    public string? Status { get; init; }

    [JsonPropertyName("public_modified")]
    public decimal? PublicModified { get; init; }
}

public sealed class ProzorroPageLink
{
    public string Offset { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public string Uri { get; init; } = string.Empty;
}