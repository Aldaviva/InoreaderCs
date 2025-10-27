namespace InoreaderCs.Auth;

/// <summary>
/// Base authentication client for Inoreader's HTTP API. See subclasses <see cref="Oauth2Client"/> and <see cref="PasswordAuthClient"/>.
/// </summary>
/// <param name="authTokenPersister">Used to save granted authentication tokens to disk so they don't have to be requested every time the app starts.</param>
/// <param name="httpClient">HTTP client used when requesting auth tokens, or <c>null</c> to use a default instance. Only the default instance will be disposed when this class is disposed.</param>
public abstract class AbstractAuthClient(IAuthTokenPersister authTokenPersister, IUnfuckedHttpClient? httpClient): IAuthClient {

    internal IUnfuckedHttpClient? OverriddenHttpClient { get; private set; } = httpClient;
    private readonly Lazy<IUnfuckedHttpClient> _defaultHttpClient = new(() => new UnfuckedHttpClient(), LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// HTTP client used when requesting auth tokens.
    /// </summary>
    public IUnfuckedHttpClient HttpClient {
        get => OverriddenHttpClient ?? _defaultHttpClient.Value;
        set {
            OverriddenHttpClient = value;
            if (_defaultHttpClient.IsValueCreated) {
                _defaultHttpClient.Value.Dispose();
            }
        }
    }

    // Using Semaphore with size 1 instead of Monitor, because Monitor releases too early on the inner await
    /// <summary>
    /// Used to not request multiple tokens at the same time, especially during parallel requests or if you have multiple auth clients that persist to the same file. If omitted, uses a default instance.
    /// </summary>
    public SemaphoreSlim Synchronizer { protected get; init; } = new(1);

    /// <summary>
    /// Used to save granted authentication tokens to disk so they don't have to be requested every time the app starts.
    /// </summary>
    protected IAuthTokenPersister AuthTokenPersister { get; } = authTokenPersister;

    /// <summary>
    /// The auth tokens most recently requested or loaded from disk.
    /// </summary>
    protected PersistedAuthTokens? CachedPersistedTokenResponses { get; set; }

    /// <inheritdoc />
    public abstract Task<IUserAuthToken> FetchValidUserToken();

    /// <inheritdoc cref="Dispose()" />
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            Synchronizer.Dispose();
            if (_defaultHttpClient.IsValueCreated) {
                _defaultHttpClient.Value.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}