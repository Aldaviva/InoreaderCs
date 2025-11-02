using InoreaderCs.Entities;
using InoreaderCs.RateLimit;
using System.Net;
using System.Text;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Requests;

internal partial class ClientRequests(InoreaderClient client):
    IInoreaderClient.IArticleMethods,
    IInoreaderClient.IFolderMethods,
    IInoreaderClient.INewsfeedMethods,
    IInoreaderClient.ISubscriptionMethods,
    IInoreaderClient.ITagMethods,
    IInoreaderClient.IUserMethods {

    private static readonly Encoding MessageEncoding = new UTF8Encoding(false, true);

    private WebTarget ApiBase => client.ApiBase;

    internal event EventHandler<IEnumerable<StreamState>>? TagAndFolderStatesListed;

    /// <exception cref="InoreaderException"></exception>
    private async Task<DetailedArticles> ListArticlesDetailed(StreamId stream, int maxArticles, DateTimeOffset? minTime, StreamId? subtract, StreamId? intersect, PaginationToken? pagination,
                                                              bool sortAscending, bool showFolders, bool showAnnotations, CancellationToken cancellationToken) {
        try {
            Task<DetailedArticles> articlesTask = ApiBase
                .Path("stream/contents/{streamId}")
                .ResolveTemplate("streamId", stream)
                .QueryParam("n", maxArticles.Clip(1, 200))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", sortAscending ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("includeAllDirectStreamIds", showFolders)
                .QueryParam("annotations", Convert.ToInt32(showAnnotations)) // docs are wrong, "true" is ignored
                .Get<DetailedArticles>(cancellationToken);

            Task<LabelNameCache.Labels> labelNamesTask = client.LabelNameCache.GetLabelNames(cancellationToken);

            DetailedArticles      articles = await articlesTask.ConfigureAwait(false);
            LabelNameCache.Labels labels   = await labelNamesTask.ConfigureAwait(false);

            foreach (Article article in articles.Articles) {
                await article.SetCategories(labels).ConfigureAwait(false);
            }

            return articles;
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list articles in stream {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<BriefArticles> ListArticlesBrief(StreamId stream, int maxArticles, DateTimeOffset? minTime, StreamId? subtract, StreamId? intersect, PaginationToken? pagination,
                                                        bool sortAscending, bool showFolders, CancellationToken cancellationToken) {
        try {
            return await ApiBase
                .Path("stream/items/ids")
                .QueryParam("n", maxArticles.Clip(1, 1000))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", sortAscending ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("s", stream)
                .QueryParam("includeAllDirectStreamIds", showFolders)
                .Get<BriefArticles>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list article IDs in feed {stream}");
        }
    }

    /// <returns>Length of <paramref name="articleIds"/></returns>
    /// <exception cref="InoreaderException"></exception>
    private async Task<int> MarkArticles(StreamId label, bool removeLabel, CancellationToken cancellationToken, params IEnumerable<string> articleIds) {
        try {
            IReadOnlyList<KeyValuePair<string, string>> articleIdFormParams = articleIds.Select(id => new KeyValuePair<string, string>("i", id)).ToList();

            if (articleIdFormParams.Count != 0) {
                (await ApiBase
                        .Path("edit-tag")
                        .Post(new FormUrlEncodedContent(articleIdFormParams.Prepend(new KeyValuePair<string, string>(removeLabel ? "r" : "a", label.Id))), cancellationToken)
                        .ConfigureAwait(false))
                    .Dispose();
            }
            return articleIdFormParams.Count;
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to {(removeLabel ? "untag" : "tag")} articles with tag {label}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    internal async Task<IEnumerable<StreamState>> ListTagAndFolderStates(CancellationToken cancellationToken) {
        try {
            IList<StreamState> tagAndFolderStates = (await ApiBase
                    .Path("tag/list")
                    .QueryParam("types", 1)
                    .QueryParam("counts", 1)
                    .Get<TagList>(cancellationToken)
                    .ConfigureAwait(false))
                .Tags
                .ToList();

            TagAndFolderStatesListed?.Invoke(this, tagAndFolderStates);
            return tagAndFolderStates;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list tag and folder states");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task MarkAllArticlesAsRead(StreamId stream, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) {
        try {
            (await ApiBase
                    .Path("mark-all-as-read")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"]  = stream.Id,
                        ["ts"] = maxSeenArticleTime.ToUnixTimeMicroseconds().ToString()
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to mark all articles as read in {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task RenameFolderOrTag(StreamId stream, string newName, CancellationToken cancellationToken) {
        try {
            (await ApiBase
                    .Path("rename-tag")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"]    = stream.Id,
                        ["dest"] = newName
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to rename folder or tag {stream} to {newName}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task DeleteFolderOrTag(StreamId stream, CancellationToken cancellationToken = default) {
        try {
            (await ApiBase
                    .Path("disable-tag")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"] = stream.Id
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
            // If the stream did not already exist, the response body is "Error=Tag not found!" instead of "OK", but I don't care because either way it successfully doesn't exist now.
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to delete folder or tag {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task ModifySubscription(StreamId stream, SubscriptionEditAction action, string? newTitle, string? newFolder, string? removeFromFolder, CancellationToken cancellationToken) {
        try {
            (await ApiBase
                    .Path("subscription/edit")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string?> {
                        ["ac"] = action.ToString().ToLowerInvariant(),
                        ["s"]  = stream.Id,
                        ["t"]  = newTitle,
                        ["a"]  = newFolder != null ? StreamId.ForFolder(newFolder).Id : null,
                        ["r"]  = removeFromFolder != null ? StreamId.ForFolder(removeFromFolder).Id : null
                    }.Compact()), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to modify feed {stream}");
        }
    }

    private enum SubscriptionEditAction {

        Subscribe, // Incorrectly documented as "follow"
        Edit,
        Unsubscribe // Incorrectly documented as "unfollow", found in Android app in com.innologica.inoreader.httpreq.MessageToServer.SendEditSubscriptionToServer(List<NameValuePair>,String)

    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<UnreadCountResponses> GetUnreadCounts(CancellationToken cancellationToken = default) {
        try {
            return await ApiBase.Path("unread-count")
                .Get<UnreadCountResponses>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get unread counts");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<LabelUnreadCounts> GetLabelUnreadCounts(bool tagsInsteadOfFolders, CancellationToken cancellationToken) {
        Task<UnreadCountResponses>  unreadCountsTask = GetUnreadCounts(cancellationToken);
        Task<LabelNameCache.Labels> labelNamesTask   = client.LabelNameCache.GetLabelNames(cancellationToken);

        UnreadCountResponses unreadCounts = await unreadCountsTask.ConfigureAwait(false);
        ISet<string>         folderNames  = (await labelNamesTask.ConfigureAwait(false)).Folders;

        // ReSharper disable once RedundantEnumerableCastCall - it's changing nullability, so it's not redundant, and it prevents warnings
        return new LabelUnreadCounts(unreadCounts.UnreadCounts
                .Select(response => (response, labelNameOrNull: response.Id.LabelName))
                .Where(responseWithLabel => responseWithLabel.labelNameOrNull is not null)
                .Cast<(UnreadCountResponse response, string labelName)>()
                .Where(responseWithLabel => tagsInsteadOfFolders != folderNames.Contains(responseWithLabel.labelName))
                .ToDictionary(responseWithLabel => responseWithLabel.labelName,
                    responseWithLabel => new StreamUnreadState(responseWithLabel.response.Count, responseWithLabel.response.NewestArticleTime)),
            unreadCounts.Max);
    }

    private static InoreaderException TransformError(HttpException cause, string message) => cause switch {
        ForbiddenException or NotAuthorizedException              => new InoreaderException.Unauthorized("Inoreader auth failure", cause),
        ClientErrorException { StatusCode: (HttpStatusCode) 429 } => new InoreaderException.RateLimited((RateLimitStatistics) cause.RequestProperties![RateLimitReader.RequestPropertyKey]!, cause),
        _ => new InoreaderException(message + (cause is WebApplicationException { ResponseBody: { } body } ? ": " + MessageEncoding.GetString(body.Span
#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
                .ToArray()
#endif
        ).Trim().TrimStart(1, "Error=") : null), cause)
    };

}