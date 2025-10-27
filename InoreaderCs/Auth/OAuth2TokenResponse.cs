namespace InoreaderCs.Auth;

/// <summary>
/// Response of a granted user OAuth2 token from the Inoreader HTTP API.
/// </summary>
public record OAuth2TokenResponse {

    /// <summary>
    /// The user OAuth2 token, to be sent in future requests.
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Used to create a new OAuth2 access token when it expires in the future.
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// When the OAuth2 access token will expire.
    /// </summary>
    public required DateTimeOffset Expiration { get; set; }

}