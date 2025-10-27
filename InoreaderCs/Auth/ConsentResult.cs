namespace InoreaderCs.Auth;

/// <summary>
/// The response from Inoreader's API server to the client app after a user grants consent during an OAuth2 authorization flow while using <see cref="Oauth2Client"/>, containing either a successful <paramref name="AuthorizationCode"/> or an <paramref name="ErrorCode"/>.
/// </summary>
/// <param name="AuthorizationCode">If successful, code that can be exchanged for an auth token and a refresh token.</param>
/// <param name="CsrfToken">From the client app when the authorization flow was initiated.</param>
/// <param name="ErrorCode">If unsuccessful, a unique string like <c>access_denied</c> for the error.</param>
/// <param name="ErrorMessage">If unsuccessful, a description of <paramref name="ErrorCode"/>.</param>
public readonly record struct ConsentResult(string? AuthorizationCode, string? CsrfToken, string? ErrorCode, string? ErrorMessage);