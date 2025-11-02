using System.Net.Http.Headers;

namespace InoreaderCs.Auth;

/// <summary>
/// A user authentication token for the Inoreader HTTP API.
/// </summary>
public interface IUserAuthToken {

    /// <summary>
    /// The <c>Authorization</c> header to send in API requests.
    /// </summary>
    AuthenticationHeaderValue AuthenticationHeaderValue { get; }

    /// <summary>
    /// Additional headers to send with API requests, excluding <c>Authorization</c>.
    /// </summary>
    IDictionary<string, object>? RequestHeaders { get; }

}

/// <summary>
/// An OAuth2 user authentication token for the Inoreader HTTP API.
/// </summary>
/// <param name="userToken">The OAuth2 access token.</param>
public class Oauth2UserToken(string userToken): IUserAuthToken {

    /// <inheritdoc />
    public AuthenticationHeaderValue AuthenticationHeaderValue => new("Bearer", userToken);

    /// <inheritdoc />
    public IDictionary<string, object>? RequestHeaders => null;

}

/// <summary>
/// <para>A user authentication token generated from a username and password for the Inoreader HTTP API.</para>
/// <para>For the OAuth2 equivalent, see <see cref="Oauth2UserToken"/>.</para>
/// </summary>
/// <param name="userToken">The user auth token.</param>
/// <param name="appId">The registered ID of an Inoreader client app.</param>
/// <param name="appKey">The registered secret of an Inoreader client app.</param>
public class UserPasswordToken(string userToken, int appId, string appKey): IUserAuthToken {

    /// <inheritdoc />
    public AuthenticationHeaderValue AuthenticationHeaderValue => new("GoogleLogin", "auth=" + userToken);

    /// <inheritdoc />
    public IDictionary<string, object>? RequestHeaders { get; } = new Dictionary<string, object> {
        ["AppId"]  = appId,
        ["AppKey"] = appKey
    };

}