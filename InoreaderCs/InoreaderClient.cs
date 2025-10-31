using InoreaderCs.Auth;
using InoreaderCs.Marshal;
using InoreaderCs.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unfucked.HTTP.Config;

namespace InoreaderCs;

/// <summary>
/// <para>Client for the Inoreader HTTP API.</para>
/// <para>To get started, construct a new instance of this class. Pass either an <see cref="Oauth2Client"/> or <see cref="PasswordAuthClient"/> instance constructed with your registered app details and any other credentials required.</para>
/// <para>Once you have an instance, you can send API requests by calling methods like <see cref="Newsfeed"/>.<c>ListArticlesDetailed</c>.</para>
/// </summary>
/// <remarks>See <see href="https://www.inoreader.com/developers/"/></remarks>
public class InoreaderClient: IInoreaderClient {

    internal static readonly Uri ApiRoot = new("https://www.inoreader.com/");

    private static readonly JsonConverter<DateTimeOffset?> StringToDateTimeOffsetReader = new DateTimeOffsetReader();

    /// <summary>
    /// JSON response deserialization preferences
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) {
        PropertyNamingPolicy              = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        Converters = {
            new JsonStringEnumConverter(),
            StringToDateTimeOffsetReader,
            new NonNullableValueReader<DateTimeOffset>(StringToDateTimeOffsetReader),
            new StringToStreamIdConverter()
        }
    };

    private readonly  IUnfuckedHttpClient _httpClient;
    private readonly  bool                _disposeHttpClient;
    internal readonly Requests.Requests   Requests;
    private readonly  RateLimitReader     _rateLimitReader = new();
    private readonly  object              _eventLock       = new();
    internal readonly LabelNameCache      LabelNameCache;

    /// <inheritdoc />
    public WebTarget ApiBase { get; set; }

    /// <summary>
    /// Construct a new Inoreader API client instance, with one given type of authentication.
    /// </summary>
    /// <param name="authClient">Provides authentication, using either OAuth2 (<see cref="Oauth2Client"/>) or a user's password (<see cref="PasswordAuthClient"/>).</param>
    /// <param name="httpClient">Optional HTTP client if you want to customize how requests and responses are handled, or <c>null</c> to use a default instance</param>
    /// <param name="disposeHttpClient">Whether <paramref name="httpClient"/> will be disposed along with this object. By default, it is only disposed when a custom <paramref name="httpClient"/> was provided and was not <c>null</c>.</param>
    public InoreaderClient(IAuthClient authClient, IUnfuckedHttpClient? httpClient = null, bool? disposeHttpClient = null) {
        _disposeHttpClient = disposeHttpClient ?? httpClient is null;
        _httpClient        = httpClient ?? new UnfuckedHttpClient();
        if (authClient is AbstractAuthClient { OverriddenHttpClient: null } auth) {
            auth.HttpClient = _httpClient;
        }

        ApiBase = _httpClient
            .Target(ApiRoot)
            .Register(new AuthRequestFilter(authClient))
            .Register(_rateLimitReader)
            .Property(PropertyKey.JsonSerializerOptions, JsonOptions)
            .Path("reader/api/0")
            .Accept("application/json");

        Requests       = new Requests.Requests(this);
        LabelNameCache = new LabelNameCache(this, TimeSpan.FromHours(1));
    }

    /// <inheritdoc />
    public IInoreaderClient.IArticleMethods Articles => Requests;

    /// <inheritdoc />
    public IInoreaderClient.IFolderMethods Folders => Requests;

    /// <inheritdoc />
    public IInoreaderClient.INewsfeedMethods Newsfeed => Requests;

    /// <inheritdoc />
    public IInoreaderClient.ISubscriptionMethods Subscriptions => Requests;

    /// <inheritdoc />
    public IInoreaderClient.ITagMethods Tags => Requests;

    /// <inheritdoc />
    public IInoreaderClient.IUserMethods Users => Requests;

    /// <inheritdoc />
    public event EventHandler<RateLimitStatistics>? RateLimitStatisticsReceived {
        add {
            lock (_eventLock) {
                _rateLimitReader.StatisticsReceived += value;
            }
        }
        remove {
            lock (_eventLock) {
                _rateLimitReader.StatisticsReceived -= value;
            }
        }
    }

    /// <inheritdoc cref="Dispose()" />
    protected virtual void Dispose(bool disposing) {
        if (disposing && _disposeHttpClient) {
            _httpClient.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}