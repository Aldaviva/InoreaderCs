using InoreaderCs.Marshal;
using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Unread counts and latest article timestamp high water marks for folders, tags, and the entire newsfeed.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/unread-counts"/></remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TagState), "tag")]
[JsonDerivedType(typeof(FolderState), "folder")]
[JsonDerivedType(typeof(ActiveSearchState), "active_search")]
public record StreamState {

    [JsonInclude]
    internal StreamId Id { get; init; } = null!;

    // public required string SortId { get; init; }

}

/// <summary>
/// Unread counts and timestamp high water mark for a folder or tag.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/unread-counts"/></remarks>
public abstract record LabelState: StreamState {

    /// <summary>
    /// The folder or tag's name, with no prefix.
    /// </summary>
    public string Name => Id.Id.Split(['/'], 4)[3];

    /// <summary>
    /// The number of unread articles in this folder or tag.
    /// </summary>
    [JsonPropertyName("unread_count")]
    public int? UnreadCount { get; init; }

    /// <summary>
    /// The number of articles in this folder or tag which the user has never loaded, looked at, or marked as read.
    /// </summary>
    [JsonPropertyName("unseen_count")]
    public int? UnseenCount { get; init; }

}

/// <summary>
/// Unread counts and timestamp high water mark for a folder.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/unread-counts"/></remarks>
public record FolderState: LabelState;

/// <summary>
/// Unread counts and timestamp high water mark for a tag.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/unread-counts"/></remarks>
public record TagState: LabelState {

    /// <summary>
    /// Whether or not the tag is pinned in the sidebar, which shows the tag on the Saved page's left navigation bar.
    /// </summary>
    [JsonPropertyName("pinned")]
    [JsonConverter(typeof(NumberToBooleanConverter))]
    public bool IsPinned { get; init; }

    /// <summary>
    /// The total number of articles that have this tag.
    /// </summary>
    [JsonPropertyName("article_count")]
    public int ArticleCount { get; init; }

    /// <summary>
    /// The number of articles crawled today which had this tag added to them.
    /// </summary>
    [JsonPropertyName("article_count_today")]
    public int ArticleCountToday { get; init; }

}

internal record ActiveSearchState: TagState;