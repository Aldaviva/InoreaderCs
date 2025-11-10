using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// A followed feed.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/subscription-list"/></remarks>
public record Subscription {

    /// <inheritdoc cref="Subscription" />
    public Subscription() {
        _folders = new Lazy<ISet<string>>(() => new HashSet<string>(Categories.Select(folder => folder.Label)), LazyThreadSafetyMode.PublicationOnly);
    }

    // public required StreamId Id { get; init; }

    /// <summary>
    /// The name or title of the subscription. Defaults to the RSS channel's <c>&lt;title&gt;</c> value, but can be changed by the Inoreader user.
    /// </summary>
    public required string Title { get; init; }

    [JsonInclude]
    private IReadOnlyList<MinimalFolder> Categories { get; init; } = [];

    private readonly Lazy<ISet<string>> _folders;

    /// <summary>
    /// Set of zero or more names of folders that the subscription belongs to.
    /// </summary>
    public ISet<string> Folders => _folders.Value;

    // public required string SortId { get; init; }

    // <c>firstitemmsec</c> seems to be completely broken because the API server returns nonsense dates. They should all be roughly 30 days ago, but in my account the values range from 73 days ago to 2252 days ago. My parsing is correct with respect to microsecond precision, so it's not a ranging problem.
    /*/// <summary>
    /// <para>The time limit of when articles can still be marked as unread. Any articles with a <see cref="BaseArticle.CrawlTime"/> older than this are too old and will be forced to be marked as read, with no way to mark them as unread.</para>
    /// <para>This is generally about 30 days ago.</para>
    /// </summary>
    [JsonPropertyName("firstitemmsec")]
    public required DateTimeOffset OldestPossibleUnreadTime { get; init; }*/

    /// <summary>
    /// The URL of the RSS or Atom XML document of this feed. For the feed's web page, see <see cref="PageUrl"/>.
    /// </summary>
    [JsonPropertyName("url")]
    public required Uri FeedUrl { get; init; }

    /// <summary>
    /// The URL of the feed's corresponding web page. For the RSS/Atom XML URL, see <see cref="FeedUrl"/>.
    /// </summary>
    [JsonPropertyName("htmlUrl")]
    public required Uri PageUrl { get; init; }

    /// <summary>
    /// The URL of a 16×16px icon for this feed.
    /// </summary>
    [JsonPropertyName("iconUrl")]
    public required Uri FaviconUrl { get; init; }

}