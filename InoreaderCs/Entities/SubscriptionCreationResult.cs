using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// The response received by the client when a new subscription to a feed is added.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/add-subscription"/></remarks>
public record SubscriptionCreationResult {

    [JsonInclude]
    private int NumResults { get; init; }

    /// <summary>
    /// <c>true</c> if either the subscription was successful, or if the feed was a duplicate that already had an existing subscription so it was ignored, or <c>false</c> if there was an error adding the subscription, such as an invalid RSS XML document.
    /// </summary>
    public bool IsSuccesfullySubscribed => NumResults != 0;

    /// <summary>
    /// The title of the feed, taken from its channel's <c>&lt;title&gt; element.</c>
    /// </summary>
    [JsonPropertyName("streamName")]
    public required string FeedName { get; init; }

    // public required string StreamId { get; init; }

}