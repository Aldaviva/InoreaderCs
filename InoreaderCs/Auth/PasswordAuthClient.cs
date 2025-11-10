using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Auth;

/// <summary>
/// 2-legged Inoreader authentication client that logs in with a user's email address, password, and a registered API app's ID and secret key, not OAuth2.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/auth"/></remarks>
public class PasswordAuthClient: AbstractAuthClient {

    private readonly ILogger<PasswordAuthClient> _logger;
    private readonly PasswordAuthParameters      _passwordAuthParameters;

    /// <summary>
    /// Create a new Inoreader authentication client that logs in with a user's email address, password, and a registered app's ID and secret key.
    /// </summary>
    /// <param name="passwordAuthParameters">User's and app's credentials.</param>
    /// <param name="authTokenPersister">Saves and loads cached auth tokens.</param>
    /// <param name="httpClient">Optional HTTP client to use when creating an app auth token for the user.</param>
    /// <param name="loggerFactory">If you want to emit logs from this class in your logging infrastructure.</param>
    /// <remarks>Documentation: <see href="https://www.inoreader.com/developers/auth"/></remarks>
    public PasswordAuthClient(PasswordAuthParameters passwordAuthParameters, IAuthTokenPersister authTokenPersister, IHttpClient? httpClient, ILoggerFactory? loggerFactory):
        base(authTokenPersister, httpClient) {
        _passwordAuthParameters = passwordAuthParameters;
        _logger                 = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<PasswordAuthClient>();
    }

    /// <inheritdoc />
    /// <summary>
    /// <para>Get a user authentication token with a user's email address and password, not OAuth.</para>
    /// </summary>
    public override async Task<IUserAuthToken> FetchValidUserToken() {
        bool shouldSave = false;
        await Synchronizer.WaitAsync().ConfigureAwait(false);
        try {
            if (CachedPersistedTokenResponses?.PasswordAuthToken is null) {
                _logger.LogDebug("Loading saved app token...");
                CachedPersistedTokenResponses ??= new PersistedAuthTokens();
                if (await AuthTokenPersister.LoadAuthTokens().ConfigureAwait(false) is { } loadedAuthTokens) {
                    CachedPersistedTokenResponses.LoadDefaults(loadedAuthTokens);
                }
            }

            if (CachedPersistedTokenResponses.PasswordAuthToken is null) {
                _logger.LogInformation("No saved app token, creating new one...");
                string appAuthToken = await AuthorizeAppUser().ConfigureAwait(false);
                CachedPersistedTokenResponses.PasswordAuthToken = appAuthToken;
                _logger.LogInformation("Successfully created a new app auth token.");
                shouldSave = true;
            }

            if (shouldSave) {
                await AuthTokenPersister.SaveAuthTokens(CachedPersistedTokenResponses).ConfigureAwait(false);
                _logger.LogDebug("Saved auth tokens.");
            }

            return new UserPasswordToken(CachedPersistedTokenResponses.PasswordAuthToken, _passwordAuthParameters.AppId, _passwordAuthParameters.AppKey);

        } finally {
            Synchronizer.Release();
        }
    }

    /// <exception cref="ProcessingException"></exception>
    /// <exception cref="InoreaderException.Unauthorized"></exception>
    private async Task<string> AuthorizeAppUser() {
        FormUrlEncodedContent requestBody = new(new Dictionary<string, string> {
            { "Email", _passwordAuthParameters.UserEmailAddress },
            { "Passwd", _passwordAuthParameters.UserPassword },
            { "AppId", _passwordAuthParameters.AppId.ToString() },
            { "AppKey", _passwordAuthParameters.AppKey }
        });
        requestBody.Headers.ContentLanguage.Add("en_US");

        try {
            string responseBody = await HttpClient.Target(InoreaderClient.ApiRoot)
                .Path("accounts/ClientLogin")
                .Header(HttpHeaders.USER_AGENT, "Inoreader Android v7.9.6")
                .Post<string>(requestBody).ConfigureAwait(false);

            Dictionary<string, string> responseMap = responseBody.Trim()
                .Split('\n')
                .Select(line => line.Split(['='], 2))
                .ToDictionary(split => split[0], split => split[1]);

            return responseMap["Auth"];
        } catch (WebApplicationException e) {
            throw new InoreaderException.Unauthorized($"Failed to create password auth token: {(int) e.StatusCode}", e);
        } catch (ProcessingException e) {
            _logger.LogError(e, "Network or serialization error while creating password auth token");
            throw;
        }
    }

}