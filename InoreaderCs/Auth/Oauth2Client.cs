using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Unfucked.HTTP.Exceptions;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Security.Cryptography;
#endif

namespace InoreaderCs.Auth;

/// <summary>
/// <para>3-legged OAuth2 Inoreader authentication client that logs in with a registered app ID and secret and shows the user a consent page, which will grant a user access token.</para>
/// <para>You must subclass this abstract superclass to provide ways to determine your web server's callback URL and show the consent page to the user, such as launching a web browser.</para>
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/oauth"/></remarks>
public abstract class Oauth2Client: AbstractAuthClient {

    private static readonly TimeSpan   EarlyRefreshPeriod = TimeSpan.FromMinutes(5);
    private static readonly UrlBuilder InoreaderOauthBase = InoreaderClient.ApiRoot.ToBuilder().Path("oauth2");

    private readonly ILogger<Oauth2Client> _logger;
    private readonly Oauth2Parameters      _oauthParameters;

    /// <summary>
    /// Create a new Inoreader authentication client that logs in with OAuth2 and a registered app ID and secret.
    /// </summary>
    /// <param name="oauthParameters">App's credentials.</param>
    /// <param name="authTokenPersister">Saves and loads cached auth tokens.</param>
    /// <param name="httpClient">Optional HTTP client to use when creating an app auth token for the user.</param>
    /// <param name="loggerFactory">If you want to emit logs from this class in your logging infrastructure.</param>
    /// <remarks>Documentation: <see href="https://www.inoreader.com/developers/oauth"/></remarks>
    protected Oauth2Client(Oauth2Parameters oauthParameters, IAuthTokenPersister authTokenPersister, IHttpClient? httpClient, ILoggerFactory? loggerFactory): base(authTokenPersister, httpClient) {
        _oauthParameters = oauthParameters;
        _logger          = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<Oauth2Client>();
    }

    /// <summary>
    /// <para>An absolute callback URI that Inoreader will redirect to from the consent page.</para>
    /// <para>This must be a URI set in the Inoreader OAuth2 app registration settings.</para>
    /// <para>This should be a page served by your app's web server, which will receive the request that contains the granted access token.</para>
    /// <para>Example: <c>http://localhost:8080/oauth2/authorization_code</c></para>
    /// </summary>
    protected abstract Uri AuthorizationReceiverCallbackUrl { get; }

    /// <summary>
    /// Open a web browser to show the user the OAuth2 consent page and wait for them to authorize this app.
    /// </summary>
    /// <param name="consentUri">The URL of the consent page hosted by Inoreader, which should be opened in a browser.</param>
    /// <param name="codeReceiverUri">The callback URL hosted by this app, from <see cref="AuthorizationReceiverCallbackUrl"/>. The app should expect a request to this URL.</param>
    /// <param name="authorizationSuccess">Completed when the authorization flow is finished, to which you can asynchronously listen to know when to tear down the web server if it's not going to be used for anything else.</param>
    /// <returns>The result from the user consenting to the OAuth2 app authorization or not. This should be parsed from the HTTP request sent from the Inoreader API to your web server at <paramref name="codeReceiverUri"/>.</returns>
    protected abstract Task<ConsentResult> ShowConsentPageToUser(Uri consentUri, Uri codeReceiverUri, Task authorizationSuccess);

    /// <inheritdoc />
    /// <summary>
    /// <para>Do whatever it takes to make sure the OAuth2 access token managed by this class is valid now.</para>
    /// <para>If the user has not previously authorized this client, or if token refreshing fails, this will open a browser tab with the Inoreader consent screen.</para>para>
    /// <para>Otherwise, if the user has already authorized this client but the access token has expired, this will silently refresh the access token.</para>
    /// <para>Otherwise, the OAuth2 access token is already valid, so this will do nothing.</para>
    /// </summary>
    public override async Task<IUserAuthToken> FetchValidUserToken() {
        bool shouldSave = false;
        await Synchronizer.WaitAsync().ConfigureAwait(false);
        try {
            if (CachedPersistedTokenResponses?.AccessToken is null) {
                _logger.LogDebug("Loading saved auth token...");
                CachedPersistedTokenResponses ??= new PersistedAuthTokens();
                if (await AuthTokenPersister.LoadAuthTokens().ConfigureAwait(false) is { } loadedAuthTokens) {
                    CachedPersistedTokenResponses.LoadDefaults(loadedAuthTokens);
                }
            }

            if (CachedPersistedTokenResponses.AccessToken is null) {
                _logger.LogInformation("No saved auth token, starting new authorization process...");
                Oauth2TokenResponse response = await Authorize().ConfigureAwait(false);
                CachedPersistedTokenResponses.Load(response);
                _logger.LogInformation("Successfully authorized with a new auth token.");
                shouldSave = true;
            } else if (CachedPersistedTokenResponses.Expiration?.Subtract(EarlyRefreshPeriod) < DateTimeOffset.Now && CachedPersistedTokenResponses.RefreshToken is { } refreshToken) {
                try {
                    _logger.LogDebug("Saved auth token is too old (expired {expiration:F}), refreshing it...", CachedPersistedTokenResponses.Expiration);
                    CachedPersistedTokenResponses.Load(await RefreshAuthToken(refreshToken).ConfigureAwait(false));
                    _logger.LogDebug("Successfully refreshed auth token.");
                } catch (InoreaderException.Unauthorized e) {
                    _logger.LogWarning("Failed to refresh auth token ({msg}), starting new authorization process...", e.Message);
                    CachedPersistedTokenResponses.Load(await Authorize().ConfigureAwait(false));
                    _logger.LogInformation("Successfully reauthorized with a new auth token.");
                }

                shouldSave = true;
            }

            if (shouldSave) {
                await AuthTokenPersister.SaveAuthTokens(CachedPersistedTokenResponses).ConfigureAwait(false);
                _logger.LogDebug("Saved auth tokens.");
            }

            return new Oauth2UserToken(CachedPersistedTokenResponses.AccessToken!);
        } finally {
            Synchronizer.Release();
        }
    }

    /// <summary>
    /// Perform the initial consent, redirect, and auth code exchange.
    /// </summary>
    /// <returns>An OAuth2 access token, refresh token, and expiration date.</returns>
    /// <exception cref="InoreaderException.Unauthorized">User denied consent, or other OAuth2 error</exception>
    /// <exception cref="ProcessingException"></exception>
    private async Task<Oauth2TokenResponse> Authorize() {
        TaskCompletionSource<bool> onAuthorized = new();

        Uri    codeReceiverCallbackUri = AuthorizationReceiverCallbackUrl;
        string expectedCsrfToken       = Cryptography.GenerateRandomString(256);

        Uri consentUri = InoreaderOauthBase.Path("auth")
            .QueryParam(new Dictionary<string, string> {
                { "client_id", _oauthParameters.ClientId.ToString() },
                { "redirect_uri", codeReceiverCallbackUri.ToString() },
                { "response_type", "code" },
                { "scope", "read write" },
                { "state", expectedCsrfToken }
            });

        ConsentResult consentResult = await ShowConsentPageToUser(consentUri, codeReceiverCallbackUri, onAuthorized.Task).ConfigureAwait(false);

        try {
            if (consentResult.AuthorizationCode is not null && consentResult.CsrfToken is not null &&
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedCsrfToken), Encoding.UTF8.GetBytes(consentResult.CsrfToken))
#else
                Encoding.UTF8.GetBytes(expectedCsrfToken).Zip(Encoding.UTF8.GetBytes(consentResult.CsrfToken), (a, b) => a == b).Aggregate(true, (p, n) => p && n)
#endif
               ) {

                Oauth2TokenResponse authToken = await RequestOAuthToken("authorization_code", new Dictionary<string, string> {
                    { "code", consentResult.AuthorizationCode },
                    { "redirect_uri", codeReceiverCallbackUri.ToString() },
                    { "scope", "" }
                }).ConfigureAwait(false);
                onAuthorized.TrySetResult(true);
                return authToken;
            } else if (consentResult.ErrorCode is not null) {
                throw new InoreaderException.Unauthorized(consentResult.ErrorCode switch {
                    "access_denied" => "Application was denied access to your Inoreader account",
                    _               => $"{consentResult.ErrorCode}: {consentResult.ErrorMessage}"
                }, null);
            } else {
                throw new InoreaderException.Unauthorized("Wrong CSRF token, you are being hacked", null);
            }
        } catch (InoreaderException.Unauthorized e) {
            onAuthorized.TrySetException(e);
            throw;
        } catch (ProcessingException e) {
            onAuthorized.TrySetException(e);
            throw;
        }
    }

    /// <summary>
    /// Exchange an old refresh token for a new access token and refresh token.
    /// </summary>
    /// <exception cref="ProcessingException"></exception>
    /// <exception cref="InoreaderException.Unauthorized"></exception>
    private Task<Oauth2TokenResponse> RefreshAuthToken(string refreshToken) =>
        RequestOAuthToken("refresh_token", Singleton.Dictionary("refresh_token", refreshToken));

    /// <summary>
    /// Exchange some credentials (like an authorization code or refresh token) for a new access token and refresh token.
    /// </summary>
    /// <exception cref="InoreaderException.Unauthorized"></exception>
    /// <exception cref="ProcessingException"></exception>
    private async Task<Oauth2TokenResponse> RequestOAuthToken(string grantType, IEnumerable<KeyValuePair<string, string>> requestBody) {
        FormUrlEncodedContent body = new(new Dictionary<string, string>(requestBody.ToDictionary(pair => pair.Key, pair => pair.Value)) {
            { "client_id", _oauthParameters.ClientId.ToString() },
            { "client_secret", _oauthParameters.ClientSecret },
            { "grant_type", grantType }
        });

        try {
            return await HttpClient.Target(InoreaderOauthBase)
                .Path("token")
                .Post<Oauth2TokenResponse>(body).ConfigureAwait(false);
        } catch (WebApplicationException e) {
            try {
                throw new InoreaderException.Unauthorized($"Failed to get auth token: {(int) e.StatusCode} {ParseError(e.ResponseBody)?["error_description"]?.GetValue<string>()}", e);
            } catch (JsonException e2) {
                throw new ProcessingException(e2, new HttpExceptionParams(e.Verb, e.RequestUrl, e.ResponseHeaders, e.ResponseBody, e.RequestProperties));
            } catch (FormatException e2) {
                throw new ProcessingException(e2, new HttpExceptionParams(e.Verb, e.RequestUrl, e.ResponseHeaders, e.ResponseBody, e.RequestProperties));
            }
        } catch (ProcessingException e) {
            _logger.LogError(e, "Network or serialization error while requesting auth token");
            throw;
        }

        // .NET ≤ 8 can't have ref structs like ReadOnlySpan<byte> as a local variable inside async methods
        static JsonNode? ParseError(ReadOnlyMemory<byte>? body) => body is { Span: var bytes } ? JsonSerializer.Deserialize<JsonNode>(bytes) : null;
    }

}