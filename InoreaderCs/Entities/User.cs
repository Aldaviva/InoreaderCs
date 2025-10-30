using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record User {

    [JsonPropertyName("userId")]
    public int Id { get; init; }

    [JsonPropertyName("userName")]
    public string Name { get; init; }

    [JsonPropertyName("userProfileId")]
    public int ProfileId { get; init; }

    [JsonPropertyName("userEmail")]
    public string EmailAddress { get; init; }

    public bool IsBloggerUser { get; init; }

    [JsonPropertyName("signupTimeSec")]
    public DateTimeOffset signupTime { get; init; }

    public bool IsMultiLoginEnabled { get; init; }

}