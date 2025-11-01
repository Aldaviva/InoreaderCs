#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace InoreaderCs.Entities;

/// <summary>
/// See <see href="https://www.inoreader.com/developers/stream-ids"/> and <see href="https://www.inoreader.com/developers/stream-contents#:~:text=ID%20formats.-,categories,-is%20a%20list"/>
/// </summary>
internal class StreamId {

    private const string SystemIdPrefix = "user/-/state/com.google";
    private const string LabelPrefix    = "user/-/label/";

    /// <summary>All articles in your account, identical to <see cref="ReadingList"/></summary>
    public static readonly StreamId Root = new(SystemIdPrefix + "/root");

    /// <summary>All articles in your account, identical to <see cref="Root"/></summary>
    public static readonly StreamId ReadingList = new(SystemIdPrefix + "/reading-list");

    /// <summary>Articles which you have marked as read, or are more than 30 days old</summary>
    public static readonly StreamId Read = new(SystemIdPrefix + "/read");

    /// <summary>Articles which you have starred, favorited, or saved to Read Later</summary>
    public static readonly StreamId Starred = new(SystemIdPrefix + "/starred");

    /// <summary>Articles to which you have added annotations</summary>
    public static readonly StreamId Annotated = new(SystemIdPrefix + "/annotated");

    /// <summary>Articles which have been broadcast, whatever that means</summary>
    public static readonly StreamId Broadcast = new(SystemIdPrefix + "/broadcast");

    /// <summary>Articles which have been liked by other users using some social feature</summary>
    public static readonly StreamId Like = new(SystemIdPrefix + "/like");

    /// <summary>Articles which are saved web pages or something</summary>
    public static readonly StreamId SavedWebPages = new(SystemIdPrefix + "/saved-web-pages");

    private static StreamId ForLabel(string folderOrTagName) => new(LabelPrefix + folderOrTagName);

    /// <summary>A tag that you have added to an article</summary>
    public static StreamId ForTag(string tagName) => ForLabel(tagName);

    /// <summary>A folder that contains a feed</summary>
    public static StreamId ForFolder(string folderName) => ForLabel(folderName);

    /// <summary>A system state, either <see cref="Entities.ArticleState.Read"/> or <see cref="Entities.ArticleState.Starred"/></summary>
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    [return: NotNullIfNotNull(nameof(state))]
#endif
    public static StreamId? ForState(ArticleState? state) => state is null ? null : state.Value switch {
        ArticleState.Read    => Read,
        ArticleState.Starred => Starred
    };

    /// <summary>One specific feed</summary>
    public static StreamId ForFeed(Uri feedUri) => new("feed/" + feedUri.AbsoluteUri);

    /// <summary>
    /// The raw <c>tag</c> URL of the stream, such as <c>user/-/state/com.google/starred</c>.
    /// </summary>
    public string Id { get; }

    private StreamId(string id) {
        Id = id;
    }

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static StreamId Parse(string id) {
        string[] segments = id.Split(['/'], 3);

        return segments[0] switch {
            "feed" => ForFeed(FeedStreamIdToUri(id)),
            "user" => new StreamId($"{segments[0]}/-/{segments[2]}"),
            _      => throw new ArgumentOutOfRangeException($"Unable to parse stream ID \"{id}\", see the list of supported stream IDs at https://www.inoreader.com/developers/stream-ids")
        };
    }

    /// <inheritdoc cref="Id" />
    public static implicit operator string(StreamId streamId) => streamId.Id;

    /// <summary>
    /// Get the URI of the feed, if this stream ID points to a URI (e.g. <c>feed/http://feeds.arstechnica.com/arstechnica/science</c>), otherwise, returns <c>null</c> if it does not point to a feed URI.
    /// </summary>
    public Uri? FeedUri {
        get {
            if (Id.StartsWith("feed/")) {
                return FeedStreamIdToUri(Id);
            } else {
                return null;
            }
        }
    }

    /// <summary>
    /// Get the name of the folder or tag (e.g. <c>Science</c>) if this stream ID points to a label (e.g. <c>user/-/label/Science</c>), otherwise, returns <c>null</c> if it does not point to a label.
    /// </summary>
    public string? LabelName {
        get {
            if (Id.StartsWith(LabelPrefix)) {
                return Id.Substring(LabelPrefix.Length);
            } else {
                return null;
            }
        }
    }

    // public bool IsLabel => Id.StartsWith(LabelPrefix);

    private static Uri FeedStreamIdToUri(string feedStreamId) {
        return new Uri(feedStreamId.Substring("feed/".Length), UriKind.Absolute);
    }

    /// <summary>
    /// Compare streams by <see cref="Id"/>.
    /// </summary>
    /// <returns><c>true</c> if this instance has the same <see cref="Id"/> as <paramref name="other"/>, or <c>false</c> otherwise.</returns>
    private bool Equals(StreamId other) => Id == other.Id;

    /// <inheritdoc cref="Equals(StreamId)" />
    public override bool Equals(object? other) => other is not null && (ReferenceEquals(this, other) || (other.GetType() == GetType() && Equals((StreamId) other)));

    /// <returns>The hashcode of the <see cref="Id"/>.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <returns><c>true</c> if <paramref name="a"/> has the same <see cref="Id"/> as <paramref name="b"/>, or <c>false</c> otherwise.</returns>
    public static bool operator ==(StreamId? a, StreamId? b) => Equals(a, b);

    /// <returns><c>false</c> if <paramref name="a"/> has the same <see cref="Id"/> as <paramref name="b"/>, or <c>true</c> otherwise.</returns>
    public static bool operator !=(StreamId? a, StreamId? b) => !Equals(a, b);

    /// <returns>The stream ID, such as <c>user/-/state/com.google/read</c>.</returns>
    public override string ToString() => Id;

}