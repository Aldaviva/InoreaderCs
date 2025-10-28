using System.Text.Json.Serialization;

namespace InoreaderCs.Auth;

/// <summary>
/// Response of a granted user OAuth2 token from the Inoreader HTTP API.
/// </summary>
public record OAuth2TokenResponse {

    /// <summary>
    /// The user OAuth2 token, to be sent in future requests.
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    /// <summary>
    /// Used to create a new OAuth2 access token when it expires in the future.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }

    [JsonInclude, JsonPropertyName("expires_in")]
    private int ExpiresIn { get; set; }

    /// <summary>
    /// When the OAuth2 access token will expire.
    /// </summary>
    public DateTimeOffset Expiration => DateTimeOffset.Now.AddSeconds(ExpiresIn);

}