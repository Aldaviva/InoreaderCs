using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Response container envelope for listing articles.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/stream-contents"/></remarks>
public record DetailedArticles: PaginatedListResponse {

    /// <summary>
    /// The name of the stream that was requested, such as <c>Reading List</c>.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// When the stream most recently had an article added to it.
    /// </summary>
    [JsonPropertyName("updatedUsec")]
    public required DateTimeOffset UpdateTime { get; init; }

    /// <summary>
    /// Zero or more full articles.
    /// </summary>
    [JsonPropertyName("items")]
    public required IReadOnlyList<Article> Articles { get; init; }

}