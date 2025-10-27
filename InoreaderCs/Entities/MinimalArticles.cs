using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Response envelope container for <see cref="IInoreaderClient.ListMinimalArticles"/>.
/// </summary>
public record MinimalArticles: PaginatedListResponse {

    /// <summary>
    /// Zero or more minimal articles.
    /// </summary>
    [JsonPropertyName("itemRefs")]
    public required IReadOnlyList<MinimalArticle> Articles { get; init; }

}

/// <summary>
/// One news article, item, entry, or post from a feed. Contains only a minimal projection of properties from the full <see cref="Article"/> to save bandwidth, and acts as a pointer or reference to articles by only including the <see cref="ShortId"/>, <see cref="BaseArticle.CrawlTime"/>, and <see cref="FoldersAndTags"/>.
/// </summary>
public record MinimalArticle: BaseArticle {

    /// <inheritdoc />
    public override string ShortId => Id;

    [JsonInclude]
    private string Id { get; init; } = null!;

    /// <summary>
    /// <para>Zero or more folder and tag <see cref="StreamId"/>s that this article belongs to.</para>
    /// <para>Tags are always included. Folders are only included when the <see cref="IInoreaderClient.ListMinimalArticles"/> parameter <c>includeFolders</c> is <c>true</c>.</para>
    /// </summary>
    [JsonPropertyName("directStreamIds")]
    public required ISet<StreamId> FoldersAndTags { get; init; }

}