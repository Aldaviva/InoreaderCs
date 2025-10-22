using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public abstract record BaseArticle {

    public abstract string ShortId { get; }

    [JsonPropertyName("timestampUsec")]
    public required DateTimeOffset CrawlTime { get; init; }

}

public record Article: BaseArticle {

    [JsonPropertyName("id")]
    public required string LongId { get; init; }

    public override string ShortId => Convert.ToString(Convert.ToInt64(LongId.Substring("tag:google.com,2005:reader/item/".Length), 16));

    [JsonPropertyName("categories")]
    public required ISet<StreamId> Labels { get; init; }

    public required IReadOnlyList<Annotation> Annotations { get; init; }

    public required string Title { get; init; }

    [JsonPropertyName("published")]
    public required DateTimeOffset PublishTime { get; init; }

    [JsonPropertyName("updated")]
    public required DateTimeOffset? UpdateTime { get; init; }

    [JsonInclude]
    private IReadOnlyList<Link> Canonical { get; init; } = null!;

    public Uri Source => Canonical[0].Href;

    [JsonInclude]
    private Summary Summary { get; init; } = null!;

    public string Description => Summary.Content;

    public required string Author { get; init; }

    [JsonInclude] [JsonPropertyName("origin")]
    private Origin Feed { get; init; } = null!;

    public Uri FeedUrl => Feed.StreamId.FeedUri;
    public string FeedName => Feed.Title;
    public Uri FeedPageUrl => Feed.HtmlUrl;

    public bool IsStarred => Labels.Contains(StreamId.Starred);
    public bool IsRead => Labels.Contains(StreamId.Read);

    public virtual bool Equals(Article? other) => other is not null && (ReferenceEquals(this, other) || CrawlTime.Equals(other.CrawlTime));

    public override int GetHashCode() => CrawlTime.GetHashCode();

}

internal record Origin(StreamId StreamId, string Title, Uri HtmlUrl);

internal record Summary(string Content);

public record Annotation {

    public required long Id { get; init; }
    public required int Start { get; init; }
    public required int End { get; init; }

    [JsonPropertyName("added_on")] // this class switched from lower camel case to lower snake case for some reason
    public required DateTimeOffset AddedOn { get; init; }

    public required string Text { get; init; }
    public required string Note { get; init; }
    public required long UserId { get; init; }

    [JsonPropertyName("user_name")]
    public required string UserName { get; init; }

    [JsonPropertyName("user_profile_picture")]
    public required Uri UserProfilePicture { get; init; }

}