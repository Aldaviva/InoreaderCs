using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record MinimalArticles: PaginatedListResponse {

    [JsonPropertyName("itemRefs")]
    public required IReadOnlyList<MinimalArticle> Articles { get; init; }

}

public record MinimalArticle: BaseArticle {

    public override string ShortId => Id;

    [JsonInclude]
    private string Id { get; init; } = null!;

    [JsonPropertyName("directStreamIds")]
    public required IReadOnlyList<StreamId> ContainingStreams { get; init; }

}