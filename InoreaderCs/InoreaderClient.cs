using InoreaderCs.Auth;
using InoreaderCs.Entities;
using InoreaderCs.Marshal;
using InoreaderCs.RateLimiting;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unfucked;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs;

public class InoreaderClient: IInoreaderClient {

    internal static readonly Uri                  ApiBase             = new("https://www.inoreader.com/");
    internal static readonly MediaTypeHeaderValue ApplicationJsonType = new("application/json");
    private static readonly  Encoding             MessageEncoding     = new UTF8Encoding(false, true);

    private static readonly JsonConverter<DateTimeOffset?> StringToDateTimeOffsetReader = new StringToDateTimeOffsetReader();

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) {
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
    public HttpClient HttpClient { get; }

    // /// <inheritdoc />
    // public IUserAuthToken AuthToken { get; }

    public InoreaderClient(Func<Task<IUserAuthToken>> userAuthTokenProvider, HttpClient? httpClient = null, bool? disposeHttpClient = null) {
        _disposeHttpClient = disposeHttpClient ?? httpClient is null;
        HttpClient         = httpClient ?? new UnfuckedHttpClient();

        _apiTarget = HttpClient
            .Target(ApiBase)
            .Register(new InoreaderAuthenticationFilter(userAuthTokenProvider))
            .Register(_rateLimitReader)
            .Property(PropertyKey.JsonSerializerOptions, JsonOptions)
            .Path("reader/api/0")
            .Accept(ApplicationJsonType);
    }

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

    /// <inheritdoc />
    public async Task<FullArticles> ListFullArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                     PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFoldersInLabels = true, bool includeAnnotations = false) {
        try {
            return await _apiTarget.Path("stream/contents/{streamId}")
                .ResolveTemplate("streamId", stream)
                .QueryParam("n", maxArticles)
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("includeAllDirectStreamIds", includeFoldersInLabels)
                .QueryParam("annotations", includeAnnotations)
                .Get<FullArticles>()
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list articles in feed {stream}");
        }
    }

    /// <inheritdoc />
    public async Task<MinimalArticles> ListMinimalArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                           PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFoldersInLabels = true) {
        try {
            return await _apiTarget.Path("stream/items/ids")
                .QueryParam("n", maxArticles)
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("s", stream)
                .QueryParam("includeAllDirectStreamIds", includeFoldersInLabels)
                .Get<MinimalArticles>()
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list article IDs in feed {stream}");
        }
    }

    /// <inheritdoc />
    public Task LabelArticles(StreamId label, bool removeLabel = false, params IEnumerable<Article> articles) => LabelArticles(label, removeLabel, articles.Select(article => article.ShortId));

    /// <inheritdoc />
    public async Task LabelArticles(StreamId label, bool removeLabel = false, params IEnumerable<string> articleIds) {
        try {
            if (articleIds.Select(id => new KeyValuePair<string, string>("i", id)).ToList() is { Count: not 0 } ids) {
                HttpContent body = new FormUrlEncodedContent(ids.Prepend(new KeyValuePair<string, string>(removeLabel ? "r" : "a", label.ToString())));
                (await _apiTarget.Path("edit-tag").Post(body).ConfigureAwait(false)).Dispose();
            }
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to {(removeLabel ? "untag" : "tag")} articles with tag {label}");
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