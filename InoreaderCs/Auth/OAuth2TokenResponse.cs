using System.Text.Json.Serialization;

namespace InoreaderCs.Auth;

/// <summary>
/// Response of a granted user OAuth2 token from the Inoreader HTTP API.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/oauth#:~:text=Content%2Dtype%20header!-,Response%3A,-%7B%0A%20%20%22access_token%22%3A%20%22%5BACCESS_TOKEN%5D%22%2C%20%0A%20%20%22token_type"/></remarks>
public record Oauth2TokenResponse {

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