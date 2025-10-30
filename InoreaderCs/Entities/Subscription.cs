using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record Subscription {

    public required StreamId Id { get; init; }

    public required string Title { get; init; }

    [JsonPropertyName("categories")]
    public required IReadOnlyList<MinimalFolder> Folders { get; init; }

    public required string SortId { get; init; }

    [JsonPropertyName("firstitemmsec")]
    public required DateTimeOffset OldestPossibleUnreadTime { get; init; }

    [JsonPropertyName("url")]
    public required Uri FeedUrl { get; init; }

    [JsonPropertyName("htmlUrl")]
    public required Uri FeedPageUrl { get; init; }

    [JsonPropertyName("iconUrl")]
    public required Uri FeedFaviconUrl { get; init; }

}