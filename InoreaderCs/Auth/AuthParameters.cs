namespace InoreaderCs.Auth;

/// <summary>
/// Client app inputs to the OAuth2 authorization flow used by <see cref="Oauth2Client"/>, created when registering an API app in Inoreader (<see href="https://www.inoreader.com/developers/register-app"/>).
/// </summary>
/// <param name="AppId">The OAuth2 app's unique client ID.</param>
/// <param name="AppKey">The OAuth2 app's client secret key.</param>
public record Oauth2Parameters(int AppId, string AppKey);

/// <summary>
/// User and client app inputs to the password authorization flow used by <see cref="PasswordAuthClient"/>.
/// </summary>
/// <param name="UserEmailAddress">The Inoreader user's email address.</param>
/// <param name="UserPassword">The Inoreader user's password.</param>
/// <param name="AppId">The app's unique client ID.</param>
/// <param name="AppKey">The app's client secret key.</param>
public record PasswordAuthParameters(string UserEmailAddress, string UserPassword, int AppId, string AppKey);