using InoreaderCs.Entities;
using InoreaderCs.RateLimiting;
using System.Net;
using System.Text;
using Unfucked.HTTP.Exceptions;
using UnionTypes;

namespace InoreaderCs;

internal class Requests(WebTarget apiTarget):
    IInoreaderClient.IArticleMethods,
    IInoreaderClient.IFolderMethods,
    IInoreaderClient.INewsfeedMethods,
    IInoreaderClient.ISubscriptionMethods,
    IInoreaderClient.ITagMethods,
    IInoreaderClient.IUserMethods {

    private static readonly Encoding MessageEncoding = new UTF8Encoding(false, true);

    #region Shared implementations

    /// <exception cref="InoreaderException"></exception>
    private async Task<FullArticles> ListArticlesDetailed(StreamId stream, int maxArticles, DateTimeOffset? minTime, StreamId? subtract, StreamId? intersect, PaginationToken? pagination,
                                                          bool sortAscending, bool showFolders, bool includeAnnotations, CancellationToken cancellationToken) {
        try {
            return await apiTarget
                .Path("stream/contents/{streamId}")
                .ResolveTemplate("streamId", stream)
                .QueryParam("n", maxArticles.Clip(1, 200))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", sortAscending ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("includeAllDirectStreamIds", showFolders)
                .QueryParam("annotations", Convert.ToInt32(includeAnnotations)) // docs are wrong, "true" is ignored
                .Get<FullArticles>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list articles in stream {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<MinimalArticles> ListArticlesBrief(StreamId stream, int maxArticles, DateTimeOffset? minTime, StreamId? subtract, StreamId? intersect, PaginationToken? pagination,
                                                          bool sortAscending, bool showFolders, CancellationToken cancellationToken) {
        try {
            return await apiTarget
                .Path("stream/items/ids")
                .QueryParam("n", maxArticles.Clip(1, 1000))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", sortAscending ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("s", stream)
                .QueryParam("includeAllDirectStreamIds", showFolders)
                .Get<MinimalArticles>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list article IDs in feed {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task MarkArticles(StreamId label, bool removeLabel, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) {
        try {
            IReadOnlyList<KeyValuePair<string, string>> articleIdFormParams = articlesOrIds
                .Select(articleOrId => articleOrId.Switch(article => article.ShortId, id => id))
                .Select(id => new KeyValuePair<string, string>("i", id))
                .ToList();

            if (articleIdFormParams.Count != 0) {
                (await apiTarget
                        .Path("edit-tag")
                        .Post(new FormUrlEncodedContent(articleIdFormParams
                                .Prepend(new KeyValuePair<string, string>(removeLabel ? "r" : "a", label.ToString()))),
                            cancellationToken)
                        .ConfigureAwait(false))
                    .Dispose();
            }
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to {(removeLabel ? "untag" : "tag")} articles with tag {label}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<IEnumerable<StreamState>> ListTagOrFolderStates(CancellationToken cancellationToken) {
        try {
            return (await apiTarget
                    .Path("tag/list")
                    .QueryParam("types", 1)
                    .QueryParam("counts", 1)
                    .Get<TagList>(cancellationToken)
                    .ConfigureAwait(false))
                .Tags;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list tag and folder states");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task MarkAllArticlesAsRead(StreamId stream, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) {
        try {
            (await apiTarget
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
            (await apiTarget
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
            (await apiTarget
                    .Path("disable-tag")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"] = stream.Id
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to delete folder or tag {stream}");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task ModifySubscription(StreamId stream, SubscriptionEditAction action, string? newTitle, string? newFolder, string? removeFromFolder, CancellationToken cancellationToken) {
        try {
            (await apiTarget
                    .Path("subscription/edit")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string?> {
                        ["ac"] = action.ToString().ToLowerInvariant(),
                        ["s"]  = stream.Id,
                        ["t"]  = newTitle,
                        ["a"]  = newFolder,
                        ["r"]  = removeFromFolder
                    }.Compact()), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to modify feed {stream}");
        }
    }

    private enum SubscriptionEditAction {

        Follow,
        Edit,
        Unfollow

    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<UnreadCountResponses> GetUnreadCounts(CancellationToken cancellationToken = default) {
        try {
            return await apiTarget.Path("unread-count")
                .Get<UnreadCountResponses>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get unread counts");
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<LabelUnreadCounts> GetUnreadCounts(bool tagsInsteadOfFolders, CancellationToken cancellationToken) {
        Task<IEnumerable<StreamState>> listTagOrFolderStatesTask = ListTagOrFolderStates(cancellationToken);
        Task<UnreadCountResponses>     unreadCountsTask          = GetUnreadCounts(cancellationToken);

        IEnumerable<StreamState> tagOrFolderStates = await listTagOrFolderStatesTask.ConfigureAwait(false);
        UnreadCountResponses     unreadCounts      = await unreadCountsTask.ConfigureAwait(false);

        ISet<StreamId> folders = new HashSet<StreamId>(tagOrFolderStates
            .Where(state => state is FolderState)
            .Select(state => state.Id));

        return new LabelUnreadCounts(unreadCounts.UnreadCounts
                .Where(entry => tagsInsteadOfFolders != folders.Contains(entry.Id))
                .ToDictionary(response => response.Id.LabelName, response => new StreamUnreadState(response.Count, response.NewestArticleTime)),
            unreadCounts.Max);
    }

    #endregion

    #region Articles

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForState(markState), false, cancellationToken, articlesOrIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForState(unmarkState), true, cancellationToken, articlesOrIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.TagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articlesOrIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articlesOrIds);

    #endregion

    #region Folders

    /// <inheritdoc />
    async Task<IEnumerable<FolderState>> IInoreaderClient.IFolderMethods.List(CancellationToken cancellationToken) =>
        (await ListTagOrFolderStates(cancellationToken).ConfigureAwait(false)).OfType<FolderState>();

    /// <inheritdoc />
    Task<FullArticles> IInoreaderClient.IFolderMethods.ListArticlesDetailed(string inFolder, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                            PaginationToken? pagination, bool sortAscending, bool showFolders, bool includeAnnotations,
                                                                            CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ForFolder(inFolder), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            includeAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<MinimalArticles> IInoreaderClient.IFolderMethods.ListArticlesBrief(string inFolder, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                            PaginationToken? pagination, bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ForFolder(inFolder), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    Task<LabelUnreadCounts> IInoreaderClient.IFolderMethods.GetUnreadCounts(CancellationToken cancellationToken) =>
        GetUnreadCounts(false, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IFolderMethods.MarkAllArticlesAsRead(string inFolder, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ForFolder(inFolder), maxSeenArticleTime, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IFolderMethods.Rename(string folder, string newName, CancellationToken cancellationToken) {
        if (newName.Contains('/')) {
            throw new ArgumentOutOfRangeException(nameof(newName), newName, "Folder names cannot contain forward slashes");
        }
        return RenameFolderOrTag(StreamId.ForFolder(folder), newName, cancellationToken);
    }

    /// <inheritdoc />
    Task IInoreaderClient.IFolderMethods.Delete(string folder, CancellationToken cancellationToken) =>
        DeleteFolderOrTag(StreamId.ForFolder(folder), cancellationToken);

    #endregion

    #region Newsfeed

    /// <inheritdoc />
    Task<FullArticles> IInoreaderClient.INewsfeedMethods.ListArticlesDetailed(int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect, PaginationToken? pagination,
                                                                              bool sortAscending, bool showFolders, bool includeAnnotations, CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ReadingList, maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            includeAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<MinimalArticles> IInoreaderClient.INewsfeedMethods.ListArticlesBrief(int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect, PaginationToken? pagination,
                                                                              bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ReadingList, maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    async Task<NewsfeedUnreadCounts> IInoreaderClient.INewsfeedMethods.GetUnreadCounts(CancellationToken cancellationToken) {
        UnreadCountResponses unreadCounts        = await GetUnreadCounts(cancellationToken).ConfigureAwait(false);
        UnreadCountResponse? readingListResponse = null, starredResponse = null;

        foreach (UnreadCountResponse response in unreadCounts.UnreadCounts) {
            if (readingListResponse == null && response.Id == StreamId.ReadingList) {
                readingListResponse = response;
            } else if (starredResponse == null && response.Id == StreamId.Starred) {
                starredResponse = response;
            }

            if (readingListResponse != null && starredResponse != null) {
                break;
            }
        }

        return new NewsfeedUnreadCounts(
            AllArticles: new StreamUnreadState(readingListResponse!.Count, readingListResponse.NewestArticleTime),
            Starred: new StreamUnreadState(starredResponse!.Count, starredResponse.NewestArticleTime),
            unreadCounts.Max);
    }

    /// <inheritdoc />
    Task IInoreaderClient.INewsfeedMethods.MarkAllArticlesAsRead(DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ReadingList, maxSeenArticleTime, cancellationToken);

    #endregion

    #region Subscriptions

    /// <inheritdoc />
    async Task<IEnumerable<Subscription>> IInoreaderClient.ISubscriptionMethods.List(CancellationToken cancellationToken) {
        try {
            return (await apiTarget
                    .Path("subscription/list")
                    .Get<SubscriptionListResponse>(cancellationToken)
                    .ConfigureAwait(false))
                .Subscriptions;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list subscriptions");
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Rename(Uri feedLocation, string newTitle, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, newTitle, null, null, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.AddToFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, folder, null, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.RemoveFromFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, null, folder, cancellationToken);

    /// <inheritdoc />
    async Task<SubscriptionCreationResult> IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, CancellationToken cancellationToken) {
        try {
            return await apiTarget
                .Path("subscription/quickadd")
                .Post<SubscriptionCreationResult>(new FormUrlEncodedContent(new Dictionary<string, string> {
                    ["quickadd"] = StreamId.ForFeed(feedLocation)
                }), cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to subscribe to feed");
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, string? title, string? folder, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Follow, title, folder, null, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Unsubscribe(Uri feedLocation, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Unfollow, null, null, null, cancellationToken);

    #endregion

    #region Tags

    /// <inheritdoc />
    async Task<IEnumerable<TagState>> IInoreaderClient.ITagMethods.List(CancellationToken cancellationToken) =>
        (await ListTagOrFolderStates(cancellationToken).ConfigureAwait(false)).OfType<TagState>();

    /// <inheritdoc />
    Task<FullArticles> IInoreaderClient.ITagMethods.ListArticlesDetailed(string withTag, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                         PaginationToken? pagination, bool sortAscending, bool showFolders, bool includeAnnotations,
                                                                         CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ForTag(withTag), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            includeAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<MinimalArticles> IInoreaderClient.ITagMethods.ListArticlesBrief(string withTag, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                         PaginationToken? pagination, bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ForFolder(withTag), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    Task<LabelUnreadCounts> IInoreaderClient.ITagMethods.GetUnreadCounts(CancellationToken cancellationToken) =>
        GetUnreadCounts(true, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ITagMethods.MarkAllArticlesAsRead(string withTag, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ForTag(withTag), maxSeenArticleTime, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ITagMethods.Rename(string tag, string newName, CancellationToken cancellationToken) =>
        RenameFolderOrTag(StreamId.ForTag(tag), newName, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ITagMethods.Delete(string tag, CancellationToken cancellationToken) =>
        DeleteFolderOrTag(StreamId.ForTag(tag), cancellationToken);

    #endregion

    #region User

    /// <inheritdoc />
    async Task<User> IInoreaderClient.IUserMethods.GetSelf(CancellationToken cancellationToken) {
        try {
            return await apiTarget
                .Path("user-info")
                .Get<User>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get self user info");
        }
    }

    #endregion

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