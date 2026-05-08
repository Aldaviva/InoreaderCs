using InoreaderCs.Entities;

namespace InoreaderCs.Requests;

internal sealed partial class ClientRequests {

    /// <inheritdoc />
    async Task<IEnumerable<TagState>> IInoreaderClient.ITagMethods.List(CancellationToken cancellationToken) =>
        (await ListTagAndFolderStates(cancellationToken).ConfigureAwait(false)).OfTypeExactly<TagState>();

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.ITagMethods.ListArticlesDetailed(string withTag, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                             PaginationToken? pagination, bool sortAscending, bool showFolders, bool showAnnotations,
                                                                             CancellationToken cancellationToken) =>
        ListArticlesDetailed(StreamId.ForTag(withTag), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders,
            showAnnotations, cancellationToken);

    /// <inheritdoc />
    Task<DetailedArticles> IInoreaderClient.ITagMethods.ListAllArticlesDetailed(string withTag, int? maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                                bool sortAscending, bool showFolders, bool showAnnotations, CancellationToken cancellationToken) =>
        ListAllArticles<DetailedArticles, Article>((remainingArticles, page, ct) => ((IInoreaderClient.ITagMethods) this)
            .ListArticlesDetailed(withTag, remainingArticles, minTime, subtract, intersect, page, sortAscending, showFolders, showAnnotations, ct), maxArticles, cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.ITagMethods.ListArticlesBrief(string withTag, int maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                       PaginationToken? pagination, bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListArticlesBrief(StreamId.ForFolder(withTag), maxArticles, minTime, StreamId.ForState(subtract), StreamId.ForState(intersect), pagination, sortAscending, showFolders, cancellationToken);

    /// <inheritdoc />
    Task<BriefArticles> IInoreaderClient.ITagMethods.ListAllArticlesBrief(string withTag, int? maxArticles, DateTimeOffset? minTime, ArticleState? subtract, ArticleState? intersect,
                                                                          bool sortAscending, bool showFolders, CancellationToken cancellationToken) =>
        ListAllArticles<BriefArticles, BriefArticle>((remainingArticles, page, ct) => ((IInoreaderClient.ITagMethods) this)
            .ListArticlesBrief(withTag, remainingArticles, minTime, subtract, intersect, page, sortAscending, showFolders, ct), maxArticles, cancellationToken);

    /// <inheritdoc />
    Task<LabelUnreadCounts> IInoreaderClient.ITagMethods.GetUnreadCounts(CancellationToken cancellationToken) =>
        GetLabelUnreadCounts(true, cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.ITagMethods.MarkAllArticlesAsRead(string withTag, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken) =>
        MarkAllArticlesAsRead(StreamId.ForTag(withTag), maxSeenArticleTime, cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.ITagMethods.Rename(string tag, string newName, CancellationToken cancellationToken) {
        await RenameFolderOrTag(StreamId.ForTag(tag), newName, cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(tag, false, true);
        client.LabelNameCache.Edit(newName, false, false);
    }

    /// <inheritdoc />
    async Task IInoreaderClient.ITagMethods.Delete(string tag, CancellationToken cancellationToken) {
        await DeleteFolderOrTag(StreamId.ForTag(tag), cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(tag, false, true);
    }

}