using InoreaderCs.Entities;

namespace InoreaderCs.Requests;

internal sealed partial class ClientRequests {

    /// <inheritdoc />
    async Task<IEnumerable<FolderState>> IInoreaderClient.IFolderMethods.List(CancellationToken cancellationToken) =>
        (await ListTagAndFolderStates(cancellationToken).ConfigureAwait(false)).OfType<FolderState>();

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.IFolderMethods.ListArticlesDetailed(string inFolder, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                PaginationToken? pagination, bool sortAscending, bool showFolders, bool showAnnotations,
                                                                                CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ForFolder(inFolder), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            showAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.IFolderMethods.ListAllArticlesDetailed(string inFolder, int? maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                   bool sortAscending, bool showFolders, bool showAnnotations, CancellationToken cancellationToken) =>
        ListAllArticles<DetailedArticles, Article>((remainingArticles, page, ct) => ((IInoreaderClient.IFolderMethods) this)
            .ListArticlesDetailed(inFolder, remainingArticles, minTime, subtract, intersect, page, sortAscending, showFolders, showAnnotations, ct), maxArticles, cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.IFolderMethods.ListArticlesBrief(string inFolder, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                          PaginationToken? pagination, bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ForFolder(inFolder), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.IFolderMethods.ListAllArticlesBrief(string inFolder, int? maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                             bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListAllArticles<BriefArticles, BriefArticle>((remainingArticles, page, ct) => ((IInoreaderClient.IFolderMethods) this)
            .ListArticlesBrief(inFolder, remainingArticles, minTime, subtract, intersect, page, sortAscending, showFolders, ct), maxArticles, cancellationToken);

    /// <inheritdoc />
    Task<LabelUnreadCounts> IInoreaderClient.IFolderMethods.GetUnreadCounts(CancellationToken cancellationToken) =>
        GetLabelUnreadCounts(false, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IFolderMethods.MarkAllArticlesAsRead(string inFolder, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ForFolder(inFolder), maxSeenArticleTime, cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.IFolderMethods.Rename(string folder, string newName, CancellationToken cancellationToken) {
        if (newName.Contains('/')) {
            throw new ArgumentOutOfRangeException(nameof(newName), newName, "Folder names cannot contain forward slashes");
        }
        await RenameFolderOrTag(StreamId.ForFolder(folder), newName, cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(folder, true, true);
        client.LabelNameCache.Edit(newName, true, false);
    }

    /// <inheritdoc />
    async Task IInoreaderClient.IFolderMethods.Delete(string folder, CancellationToken cancellationToken) {
        await DeleteFolderOrTag(StreamId.ForFolder(folder), cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(folder, true, true);
    }

}