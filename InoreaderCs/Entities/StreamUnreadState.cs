using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record StreamUnreadState(int UnreadCount, DateTimeOffset? NewestArticleTime);

public record LabelUnreadCounts(IDictionary<string, StreamUnreadState> Subscriptions, int MaxDisplayableUnreadCount);

public record NewsfeedUnreadCounts(StreamUnreadState AllArticles, StreamUnreadState Starred, int MaxDisplayableUnreadCount);

internal record UnreadCountResponses(int Max, IEnumerable<UnreadCountResponse> UnreadCounts);

internal record UnreadCountResponse(StreamId Id, int Count, [property: JsonPropertyName("newestItemTimestampUsec")] DateTimeOffset? NewestArticleTime);