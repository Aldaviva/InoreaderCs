using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record FullArticles: PaginatedListResponse {

    [JsonPropertyName("id")]
    public required StreamId Stream { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    [JsonPropertyName("updatedUsec")]
    public required DateTimeOffset UpdateTime { get; init; }

    [JsonPropertyName("items")]
    public required IReadOnlyList<Article> Articles { get; init; }

}

public record Link(Uri Href, MediaTypeHeaderValue? Type = null);