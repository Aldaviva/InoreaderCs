using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Base class for both full articles (which contain all properties) and minimal articles (references or pointers to articles that only contain IDs and timestamps).
/// </summary>
public abstract record BaseArticle {

    /// <summary>
    /// <para>Unique ID of the article as a decimal number, without the Google-Reader–compatible prefix. Can be used any place article IDs are sent to the Inoreader API. Useful for saving bandwidth compared to the <see cref="Article.LongId"/>.</para>
    /// <para>Documentation: <see href="https://www.inoreader.com/developers/article-ids"/></para>
    /// </summary>
    public abstract string ShortId { get; }

    /// <summary>
    /// The date and time, in microseconds since the Unix epoch, when the article was first fetched from its origin feed by Inoreader.
    /// </summary>
    [JsonPropertyName("timestampUsec")]
    public required DateTimeOffset CrawlTime { get; init; }

}

/// <summary>
/// <para>One news article, item, entry, or post from a feed. Contains all properties of the article.</para>
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/stream-contents#:~:text=Description%20of%20the%20items%20list%3A"/></remarks>
public record Article: BaseArticle {

    /// <summary>
    /// <para>Full Google-Reader–compatible unique ID as a <see href="https://en.wikipedia.org/wiki/Tag_URI_scheme" /> for this article, ending with a hexadecimal number.</para>
    /// <para>Example: <c>tag:google.com,2005:reader/item/0000000af0175be2</c></para>
    /// <para>To convert this to a short ID, see <see cref="ShortId"/>.</para>
    /// <para>Documentation: <see href="https://www.inoreader.com/developers/article-ids"/></para>
    /// </summary>
    [JsonPropertyName("id")]
    public required string LongId { get; init; }

    /// <inheritdoc />
    // ExceptionAdjustment: M:System.Convert.ToInt64(System.String,System.Int32) -T:System.FormatException
    public override string ShortId => Convert.ToString(Convert.ToInt64(LongId.Substring("tag:google.com,2005:reader/item/".Length), 16));

    [JsonInclude]
    private ISet<StreamId> Categories { get; init; } = new HashSet<StreamId>();

    /// <summary>
    /// Zero or more folders that this article feed's subscription is organized into. If the <c>showFolders</c> argument to <seealso cref="IInoreaderClient.INewsfeedMethods.ListArticlesDetailed"/> is set to <c>false</c>, this will be the empty set.
    /// </summary>
    [JsonIgnore]
    public IImmutableSet<string> Folders { get; private set; } = ImmutableHashSet<string>.Empty;

    /// <summary>
    /// Zero or more tags that have been added to this article.
    /// </summary>
    [JsonIgnore]
    public IImmutableSet<string> Tags { get; private set; } = ImmutableHashSet<string>.Empty;

    internal async Task SetCategories(LabelNameCache.Labels labels) {
        HashSet<string> folders = [];
        HashSet<string> tags    = [];
        foreach (StreamId category in Categories) {
            if (category.LabelName is { } labelName) {
                if (labels.Folders.Contains(labelName)) {
                    folders.Add(labelName);
                } else { // tag or unknown, possibly due to stale tag list cache, so assume new tag
                    tags.Add(labelName);
                }
            }
        }
        Folders = folders.ToImmutableHashSet();
        Tags    = tags.ToImmutableHashSet();
    }

    /// <summary>
    /// <para>Zero or more notes that the user has added to this article.</para>
    /// <para>When fetching articles with <see cref="IInoreaderClient.INewsfeedMethods.ListArticlesDetailed"/>, this set will be empty when the <c>showAnnotations</c> parameter is <c>false</c>.</para>
    /// </summary>
    public IReadOnlyList<Annotation> Annotations { get; init; } = [];

    /// <summary>
    /// The title, summary, or headline of the article.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// When the feed reports that the article was first published.
    /// </summary>
    [JsonPropertyName("published")]
    public DateTimeOffset PublishTime { get; init; }

    /// <summary>
    /// When the feed reports that the article was most recently updated after being published, or <c>null</c> if it was never updated after being published.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTimeOffset? UpdateTime { get; init; }

    [JsonInclude]
    private IReadOnlyList<Link> Canonical { get; init; } = null!;

    /// <summary>
    /// The URL of the article's web page.
    /// </summary>
    public Uri PageUrl => Canonical[0].Href;

    [JsonInclude]
    private SummaryContainer Summary { get; init; } = null!;

    /// <summary>
    /// The description, body, or contents of the article.
    /// </summary>
    public string Description => Summary.Content;

    /// <summary>
    /// The name of the article's author.
    /// </summary>
    public required string Author { get; init; }

    [JsonInclude] [JsonPropertyName("origin")]
    private Origin Feed { get; init; } = null!;

    /// <summary>
    /// <para>The URL of the feed that this article is from, which points to an RSS or Atom XML document.</para>
    /// <para>For the article's web page URL, see <see cref="PageUrl"/>. For the feed's web page URL, see <see cref="FeedPageUrl"/>.</para>
    /// </summary>
    public Uri FeedUrl => Feed.StreamId.FeedUri!;

    /// <summary>
    /// The name or title of the feed that this article is from.
    /// </summary>
    public string FeedName => Feed.Title;

    /// <summary>
    /// <para>The URL of the web page of the feed that this article is from.</para>
    /// <para>For the RSS URL, see <see cref="FeedUrl"/>. For the article's web page URL, see <see cref="PageUrl"/>.</para>
    /// </summary>
    public Uri FeedPageUrl => Feed.HtmlUrl;

    /// <summary>
    /// <para><c>true</c> if the user added a star to this article, also known as Read Later or Saved, or <c>false</c> if the article is not starred.</para>
    /// <para>Stars can be added and removed from articles using <see cref="IInoreaderClient.IArticleMethods.MarkArticles(ArticleState,IEnumerable{Article},CancellationToken)"/> and <see cref="IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState,IEnumerable{Article},CancellationToken)"/> with the <c>markState</c> parameter set to <see cref="ArticleState.Starred"/>.</para>
    /// </summary>
    public bool IsStarred => Categories.Contains(StreamId.Starred);

    /// <summary>
    /// <c>true</c> if either the user read the article or the article is more than 30 days old, or <c>if it is unread and less than 30 days old.</c>
    /// <para>Articles can be marked read or unread using <see cref="IInoreaderClient.IArticleMethods.MarkArticles(ArticleState,IEnumerable{Article},CancellationToken)"/> and <see cref="IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState,IEnumerable{Article},CancellationToken)"/> with the <c>markState</c> parameter set to <see cref="ArticleState.Read"/>, although articles more than 30 days old cannot be marked unread.</para>
    /// </summary>
    public bool IsRead => Categories.Contains(StreamId.Read);

    /// <inheritdoc cref="Equals(object)" />
    public virtual bool Equals(Article? other) => other is not null && (ReferenceEquals(this, other) || CrawlTime.Equals(other.CrawlTime));

    /// <inheritdoc cref="GetHashCode" />
    public override int GetHashCode() => CrawlTime.GetHashCode();

    private record Origin(StreamId StreamId, string Title, Uri HtmlUrl);

    private record SummaryContainer(string Content);

    private record Link(Uri Href, MediaTypeHeaderValue? Type = null);

}