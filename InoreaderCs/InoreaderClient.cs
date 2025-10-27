using InoreaderCs.Auth;
using InoreaderCs.Marshal;
using InoreaderCs.RateLimiting;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unfucked.HTTP.Config;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs;

/// <summary>
/// <para>Client for the Inoreader HTTP API.</para>
/// <para>To get started, construct a new instance of this class. Pass either an <see cref="Oauth2Client"/> or <see cref="PasswordAuthClient"/> instance constructed with your registered app details and any other credentials required.</para>
/// <para>Once you have an instance, you can send API requests by calling methods like <see cref="ListFullArticles"/>.</para>
/// </summary>
/// <remarks>See <see href="https://www.inoreader.com/developers/"/></remarks>
public partial class InoreaderClient: IInoreaderClient {

    internal static readonly Uri      ApiBase         = new("https://www.inoreader.com/");
    private static readonly  Encoding MessageEncoding = new UTF8Encoding(false, true);

    private static readonly JsonConverter<DateTimeOffset?> StringToDateTimeOffsetReader = new StringToDateTimeOffsetReader();

    /// <summary>
    /// JSON response deserialization preferences
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
            new JsonStringEnumConverter(),
            StringToDateTimeOffsetReader,
            new NonNullableValueReader<DateTimeOffset>(StringToDateTimeOffsetReader),
            new StringToStreamIdConverter()
        }
    };

    private readonly bool            _disposeHttpClient;
    private readonly WebTarget       _apiTarget;
    private readonly RateLimitReader _rateLimitReader = new();
    private readonly object          _eventLock       = new();

    /// <inheritdoc />
    public IUnfuckedHttpClient HttpClient { get; }

    /// <summary>
    /// Construct a new Inoreader API client instance, with one given type of authentication.
    /// </summary>
    /// <param name="authClient">Provides authentication, using either OAuth2 (<see cref="Oauth2Client"/>) or a user's password (<see cref="PasswordAuthClient"/>).</param>
    /// <param name="httpClient">Optional HTTP client if you want to customize how requests and responses are handled, or <c>null</c> to use a default instance</param>
    /// <param name="disposeHttpClient">Whether <paramref name="httpClient"/> will be disposed along with this object. By default, it is only disposed when a custom <paramref name="httpClient"/> was provided and was not <c>null</c>.</param>
    public InoreaderClient(IAuthClient authClient, IUnfuckedHttpClient? httpClient = null, bool? disposeHttpClient = null) {
        _disposeHttpClient = disposeHttpClient ?? httpClient is null;
        HttpClient         = httpClient ?? new UnfuckedHttpClient();
        if (authClient is AbstractAuthClient { OverriddenHttpClient: null } auth) {
            auth.HttpClient = HttpClient;
        }

        _apiTarget = HttpClient
            .Target(ApiBase)
            .Register(new InoreaderAuthenticationFilter(authClient))
            .Register(_rateLimitReader)
            .Property(PropertyKey.JsonSerializerOptions, JsonOptions)
            .Path("reader/api/0")
            .Accept("application/json");
    }

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

    private static InoreaderException TransformError(HttpException cause, string message) {
        return cause switch {
            ForbiddenException or NotAuthorizedException              => new InoreaderException.Unauthorized("Inoreader auth failure", cause),
            ClientErrorException { StatusCode: (HttpStatusCode) 429 } => new InoreaderException.RateLimited((RateLimitStatistics) cause.RequestProperties![RateLimitReader.RequestPropertyKey]!, cause),
            _ => new InoreaderException(message + (cause is WebApplicationException { ResponseBody: { } body } ? ": " + MessageEncoding.GetString(body.Span
#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
                        .ToArray()
#endif
                ).Trim().TrimStart(1, "Error=") : null),
                cause)
        };
    }

    /// <inheritdoc cref="Dispose()" />
    protected virtual void Dispose(bool disposing) {
        if (disposing && _disposeHttpClient) {
            HttpClient.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}