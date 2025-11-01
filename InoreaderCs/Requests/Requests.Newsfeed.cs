using InoreaderCs.Entities;

namespace InoreaderCs.Requests;

internal partial class Requests {

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.INewsfeedMethods.ListArticlesDetailed(int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                  PaginationToken? pagination,
                                                                                  bool sortAscending, bool showFolders, bool showAnnotations, CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ReadingList, maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            showAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.INewsfeedMethods.ListArticlesBrief(int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect, PaginationToken? pagination,
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
            StarredArticleCount: starredResponse!.Count,
            unreadCounts.Max);
    }

    /// <inheritdoc />
    Task IInoreaderClient.INewsfeedMethods.MarkAllArticlesAsRead(DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ReadingList, maxSeenArticleTime, cancellationToken);

}