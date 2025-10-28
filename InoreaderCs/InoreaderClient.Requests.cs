using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs;

public partial class InoreaderClient {

    /// <inheritdoc />
    public async Task<FullArticles> ListFullArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                     PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFolders = true, bool includeAnnotations = false) {
        try {
            return await ApiTarget.Path("stream/contents/{streamId}")
                .ResolveTemplate("streamId", stream)
                .QueryParam("n", Math.Min(maxArticles, 200))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("includeAllDirectStreamIds", includeFolders)
                .QueryParam("annotations", Convert.ToInt32(includeAnnotations)) // docs are wrong, "true" is ignored
                .Get<FullArticles>()
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list articles in feed {stream}");
        }
    }

    /// <inheritdoc />
    public async Task<MinimalArticles> ListMinimalArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                           PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFolders = true) {
        try {
            return await ApiTarget.Path("stream/items/ids")
                .QueryParam("n", Math.Min(maxArticles, 1000))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("s", stream)
                .QueryParam("includeAllDirectStreamIds", includeFolders)
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
                (await ApiTarget.Path("edit-tag").Post(body).ConfigureAwait(false)).Dispose();
            }
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to {(removeLabel ? "untag" : "tag")} articles with tag {label}");
        }
    }

}