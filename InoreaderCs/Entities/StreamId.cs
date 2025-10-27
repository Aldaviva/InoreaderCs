namespace InoreaderCs.Entities;

/// <summary>
/// See <see href="https://www.inoreader.com/developers/stream-ids"/> and <see href="https://www.inoreader.com/developers/stream-contents#:~:text=ID%20formats.-,categories,-is%20a%20list"/>
/// </summary>
public class StreamId {

    private const string SystemIdPrefix = "user/-/state/com.google";

    /// <summary>All articles in your account, identical to <see cref="ReadingList"/></summary>
    public static StreamId Root { get; } = new(SystemIdPrefix + "/root");

    /// <summary>All articles in your account, identical to <see cref="Root"/></summary>
    public static StreamId ReadingList { get; } = new(SystemIdPrefix + "/reading-list");

    /// <summary>Articles which you have marked as read, or are more than 30 days old</summary>
    public static StreamId Read { get; } = new(SystemIdPrefix + "/read");

    /// <summary>Articles which you have starred, favorited, or saved to Read Later</summary>
    public static StreamId Starred { get; } = new(SystemIdPrefix + "/starred");

    /// <summary>Articles to which you have added annotations</summary>
    public static StreamId Annotated { get; } = new(SystemIdPrefix + "/annotated");

    /// <summary>Articles which have been broadcast, whatever that means</summary>
    public static StreamId Broadcast { get; } = new(SystemIdPrefix + "/broadcast");

    /// <summary>Articles which have been liked by other users using some social feature</summary>
    public static StreamId Like { get; } = new(SystemIdPrefix + "/like");

    /// <summary>Articles which are saved web pages or something</summary>
    public static StreamId SavedWebPages { get; } = new(SystemIdPrefix + "/saved-web-pages");

    /// <summary>A folder that contains a feed, or a tag that you have added to an article</summary>
    public static StreamId Label(string folderOrTagName) => new("user/-/label/" + folderOrTagName);

    /// <summary>One specific feed</summary>
    public static StreamId Feed(Uri feedUri) => new("feed/" + feedUri);

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
            "feed" => Feed(FeedStreamIdToUri(id)),
            "user" => new StreamId($"{segments[0]}/-/{segments[2]}"),
            _      => throw new ArgumentOutOfRangeException($"Unable to parse stream ID \"{id}\", see the list of supported stream IDs at https://www.inoreader.com/developers/stream-ids")
        };
    }

    /// <summary>
    /// Get the URI of the feed, if this stream ID points to a URI (e.g. <c>feed/http://feeds.arstechnica.com/arstechnica/science</c>), otherwise, throws an <see cref="ArgumentException"/> if it does not point to a feed URI.
    /// </summary>
    /// <exception cref="ArgumentException">if the stream ID does not start with <c>feed/</c></exception>
    public Uri FeedUri {
        get {
            if (Id.StartsWith("feed/")) {
                return FeedStreamIdToUri(Id);
            } else {
                throw new ArgumentException($"This is not a stream ID for a feed. Its ID is \"{Id}\", which does not start with \"feed/\"");
            }
        }
    }

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