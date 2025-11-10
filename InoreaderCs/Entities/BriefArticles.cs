using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Response envelope container for <see cref="IInoreaderClient.INewsfeedMethods.ListArticlesBrief"/>.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/item-ids"/></remarks>
public record BriefArticles: PaginatedListResponse {

    /// <summary>
    /// Zero or more minimal articles.
    /// </summary>
    [JsonPropertyName("itemRefs")]
    public IReadOnlyList<BriefArticle> Articles { get; init; } = [];

}

/// <summary>
/// One news article, item, entry, or post from a feed. Contains only a minimal projection of properties from the full <see cref="Article"/> to save bandwidth, and acts as a pointer or reference to articles by only including the <see cref="ShortId"/>, <see cref="BaseArticle.CrawlTime"/>, and <see cref="DirectStreamIds"/>.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/item-ids"/></remarks>
public record BriefArticle: BaseArticle {

    /// <inheritdoc cref="BriefArticle" />
    public BriefArticle() {
        _foldersAndTags = new Lazy<ISet<string>>(() => new HashSet<string>(DirectStreamIds.Select(id => id.LabelName!)), LazyThreadSafetyMode.PublicationOnly);
    }

    /// <inheritdoc />
    public override string ShortId => Id;

    [JsonInclude]
    private string Id { get; init; } = null!;

    [JsonInclude]
    private IReadOnlyList<StreamId> DirectStreamIds { get; init; } = [];

    private readonly Lazy<ISet<string>> _foldersAndTags;

    /// <summary>
    /// <para>Zero or more folder and tag <see cref="StreamId"/>s that this article belongs to.</para>
    /// <para>Tags are always included. Folders are only included when the parameter <c>showFolders</c> is <c>true</c>.</para>
    /// </summary>
    public ISet<string> FoldersAndTags => _foldersAndTags.Value;

}