using System.Security.Authentication;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Auth;

/// <summary>
/// <para>A user authentication client for the Inoreader HTTP API. Fetches user auth tokens for use in API requests.</para>
/// <para>See subclasses <see cref="Oauth2Client"/> and <see cref="PasswordAuthClient"/>.</para>
/// </summary>
public interface IAuthClient: IDisposable {

    /// <summary>
    /// Do whatever it takes to get a valid user authentication token. This can include reading a cached token from disk, refreshing an expired token, or requesting a new one, possibly with interactive user input.
    /// </summary>
    /// <returns>A user access token that is not expired</returns>
    /// <exception cref="ProcessingException">Network or deserialization error while requesting auth token.</exception>
    /// <exception cref="AuthenticationException">Wrong credentials.</exception>
    Task<IUserAuthToken> FetchValidUserToken();

}