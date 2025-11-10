using InoreaderCs.Auth;
using InoreaderCs.Marshal;
using InoreaderCs.RateLimit;
using InoreaderCs.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unfucked.HTTP.Config;

namespace InoreaderCs;

/// <summary>
/// <para>Client for the Inoreader HTTP API.</para>
/// <para>To get started, construct a new instance of this class. Pass either an <see cref="Oauth2Client"/> subclass or <see cref="PasswordAuthClient"/> instance constructed with your registered app details and any other credentials required.</para>
/// <para>Once you have an instance, you can send API requests by calling methods like <see cref="Newsfeed"/>.<c>ListArticlesDetailed</c>.</para>
/// </summary>
/// <remarks>See <see href="https://www.inoreader.com/developers/"/></remarks>
public class InoreaderClient: IInoreaderClient {

    internal static readonly Uri ApiRoot = new("https://www.inoreader.com/");

    private static readonly JsonConverter<DateTimeOffset?> DateTimeOffsetReader = new DateTimeOffsetReader();

    /// <summary>
    /// JSON response deserialization preferences
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) {
        PropertyNamingPolicy              = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        Converters = {
            new JsonStringEnumConverter(),
            DateTimeOffsetReader,
            DateTimeOffsetReader.ToNonNullable(),
            new StringToStreamIdConverter()
        }
    };

    private readonly  IHttpClient     _httpClient;
    private readonly  bool            _disposeHttpClient;
    internal readonly ClientRequests  Requests;
    private readonly  RateLimitReader _rateLimitReader = new();
    private readonly  object          _eventLock       = new();
    internal readonly LabelNameCache  LabelNameCache;

    /// <inheritdoc />
    public IWebTarget ApiBase { get; set; }

    /// <summary>
    /// Construct a new Inoreader API client instance, with options including the mandatory authentication client.
    /// </summary>
    /// <param name="options">Parameters for this instance, including the mandatory <see cref="InoreaderOptions.AuthClient"/>.</param>
    /// <remarks>See <see href="https://www.inoreader.com/developers/"/></remarks>
    public InoreaderClient(InoreaderOptions options) {
        _disposeHttpClient = options.DisposeHttpClient ?? options.HttpClient is null;
        _httpClient        = options.HttpClient ?? new UnfuckedHttpClient();
        if (options.AuthClient is AbstractAuthClient { OverriddenHttpClient: null } auth) {
            auth.HttpClient = _httpClient;
        }

        ApiBase = _httpClient
            .Target(ApiRoot)
            .Register(new AuthRequestFilter(options.AuthClient))
            .Register(_rateLimitReader)
            .Property(PropertyKey.JsonSerializerOptions, JsonOptions)
            .Path("reader/api/0")
            .Accept("application/json");

        Requests       = new ClientRequests(this);
        LabelNameCache = new LabelNameCache(this, options.LabelNameCacheDuration);
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