namespace InoreaderCs.Auth;

/// <summary>
/// Client app inputs to the OAuth2 authorization flow used by <see cref="Oauth2Client"/>, created when registering an API app in Inoreader (<see href="https://www.inoreader.com/developers/register-app"/>).
/// </summary>
/// <param name="AppId">The OAuth2 app's unique ID</param>
/// <param name="AppKey">The OAuth2 app's secret key</param>
public record Oauth2Parameters(int AppId, string AppKey);

/// <summary>
/// User and client app inputs to the password authorization flow used by <see cref="PasswordAuthClient"/>, created when registering an Inoreader user account and an API app in Inoreader (<see href="https://www.inoreader.com/developers/register-app"/>).
/// </summary>
/// <param name="UserEmailAddress">The Inoreader user's email address</param>
/// <param name="UserPassword">The Inoreader user's password</param>
/// <param name="AppId">The OAuth2 app's unique ID</param>
/// <param name="AppKey">The OAuth2 app's secret key</param>
public record PasswordAuthParameters(string UserEmailAddress, string UserPassword, int AppId, string AppKey);