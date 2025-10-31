using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Basic information about the account of the current user.
/// </summary>
public record User {

    /// <summary>
    /// Numeric user ID.
    /// </summary>
    [JsonPropertyName("userId")]
    public int Id { get; init; }

    /// <summary>
    /// User's full name, also called real name.
    /// </summary>
    [JsonPropertyName("userName")]
    public required string Name { get; init; }

    /// <summary>
    /// Seems to always be the same as <see cref="Id"/>.
    /// </summary>
    [JsonPropertyName("userProfileId")]
    public int ProfileId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// Undocumented.
    /// </summary>
    public bool IsBloggerUser { get; init; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    [JsonPropertyName("signupTimeSec")]
    public DateTimeOffset SignupTime { get; init; }

    /// <summary>
    /// Undocumented.
    /// </summary>
    public bool IsMultiLoginEnabled { get; init; }

}