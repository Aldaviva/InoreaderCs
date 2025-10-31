using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// How many unread articles are in the newsfeed, folder, or tag, and when was the most recent article crawled.
/// </summary>
/// <param name="UnreadCount">Number of unread articles.</param>
/// <param name="NewestArticleTime">Most recent article crawl time.</param>
public record StreamUnreadState(int UnreadCount, DateTimeOffset? NewestArticleTime);

/// <summary>
/// Mapping from folder or tag name to its number of unread articles and latest crawl time.
/// </summary>
/// <param name="Subscriptions">Map with the folder or tag name for the key (without any prefix), and the unread state as the value.</param>
/// <param name="MaxDisplayableUnreadCount">The largest number of unread articles that can be displayed before being capped. For example, if this is <c>1000</c>, then all numbers of unread articles greater than 1000 will be reported as 1000, so you should render that case as <c>1000+</c> to illustrate the clipping.</param>
public record LabelUnreadCounts(IDictionary<string, StreamUnreadState> Subscriptions, int MaxDisplayableUnreadCount);

/// <summary>
/// How many unread articles and unread starred articles are in the user's entire newsfeed.
/// </summary>
/// <param name="AllArticles">Total number of unread articles in the user's entire newsfeed.</param>
/// <param name="Starred">Number of articles that are both unread and starred in the user's entire newsfeed.</param>
/// <param name="MaxDisplayableUnreadCount">The largest number of unread articles that can be displayed before being capped. For example, if this is <c>1000</c>, then all numbers of unread articles greater than 1000 will be reported as 1000, so you should render that case as <c>1000+</c> to illustrate the clipping.</param>
public record NewsfeedUnreadCounts(StreamUnreadState AllArticles, StreamUnreadState Starred, int MaxDisplayableUnreadCount);

internal record UnreadCountResponses(int Max, IEnumerable<UnreadCountResponse> UnreadCounts);

internal record UnreadCountResponse(StreamId Id, int Count, [property: JsonPropertyName("newestItemTimestampUsec")] DateTimeOffset? NewestArticleTime);