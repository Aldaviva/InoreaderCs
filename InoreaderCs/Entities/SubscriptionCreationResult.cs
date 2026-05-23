using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// The response received by the client when a new subscription to a feed is added.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/add-subscription"/></remarks>
public sealed record SubscriptionCreationResult {

    [JsonInclude]
    private int NumResults { get; init; }

    /// <summary>
    /// <c>true</c> if either the subscription was successful, or if the feed was a duplicate that already had an existing subscription so it was ignored, or <c>false</c> if there was an error adding the subscription, such as an invalid RSS XML document or ≥ 400 HTTP status code for the feed response.
    /// </summary>
    public bool IsSuccesfullySubscribed => NumResults != 0;

    /// <summary>
    /// The title of the feed, taken from its channel's <c>&lt;title&gt;</c> element, or <c>null</c> if <see cref="IsSuccesfullySubscribed"/> is <c>false</c>.
    /// </summary>
    [JsonPropertyName("streamName")]
    public string? FeedName { get; init; }

    // public required string StreamId { get; init; }

}