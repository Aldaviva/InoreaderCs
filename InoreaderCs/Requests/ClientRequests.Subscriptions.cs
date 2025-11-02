using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Requests;

internal partial class ClientRequests {

    /// <inheritdoc />
    async Task<IEnumerable<Subscription>> IInoreaderClient.ISubscriptionMethods.List(CancellationToken cancellationToken) {
        try {
            return (await ApiBase
                    .Path("subscription/list")
                    .Get<SubscriptionListResponse>(cancellationToken)
                    .ConfigureAwait(false))
                .Subscriptions;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list subscriptions");
        }
    }

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.ISubscriptionMethods.ListArticlesDetailed(Uri feedLocation, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                      PaginationToken? pagination, bool sortAscending, bool showFolders, bool showAnnotations,
                                                                                      CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ForFeed(feedLocation), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, showAnnotations,
            cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.ISubscriptionMethods.ListArticlesBrief(Uri feedLocation, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                PaginationToken? pagination, bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ForFeed(feedLocation), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Rename(Uri feedLocation, string newTitle, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, newTitle, null, null, cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.ISubscriptionMethods.AddToFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) {
        await ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, folder, null, cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(folder, true, false);
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.RemoveFromFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, null, folder, cancellationToken);

    /// <inheritdoc />
    async Task<SubscriptionCreationResult> IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, CancellationToken cancellationToken) {
        try {
            return await ApiBase
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
    async Task IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, string? title, string? folder, CancellationToken cancellationToken) {
        await ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Subscribe, title, folder, null, cancellationToken).ConfigureAwait(false);
        if (folder is not null) {
            client.LabelNameCache.Edit(folder, true, false);
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Unsubscribe(Uri feedLocation, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Unsubscribe, null, null, null, cancellationToken);

    /// <inheritdoc />
    async Task<SubscriptionUnreadCounts> IInoreaderClient.ISubscriptionMethods.GetUnreadCounts(CancellationToken cancellationToken) {
        UnreadCountResponses unreadCounts = await GetUnreadCounts(cancellationToken).ConfigureAwait(false);

        // ReSharper disable once RedundantEnumerableCastCall - it's changing nullability, so it's not redundant, and it prevents warnings
        return new SubscriptionUnreadCounts(unreadCounts.UnreadCounts
                .Select(response => (response, feedUriOrNull: response.Id.FeedUri))
                .Where(responseWithLabel => responseWithLabel.feedUriOrNull is not null)
                .Cast<(UnreadCountResponse response, Uri feedUri)>()
                .ToDictionary(responseWithLabel => responseWithLabel.feedUri,
                    responseWithLabel => new StreamUnreadState(responseWithLabel.response.Count, responseWithLabel.response.NewestArticleTime)),
            unreadCounts.Max);
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.MarkAllArticlesAsRead(Uri feedLocation, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ForFeed(feedLocation), maxSeenArticleTime, cancellationToken);

}