using InoreaderCs.Entities;

namespace InoreaderCs.Requests;

internal partial class ClientRequests {

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForState(markState)!, false, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).MarkArticles(markState, articles.Select(article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForState(unmarkState)!, true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).UnmarkArticles(unmarkState, articles.Select(article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.IArticleMethods.TagArticles(string tag, IEnumerable<string> articleIds, CancellationToken cancellationToken) {
        int articleCount = await MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articleIds).ConfigureAwait(false);
        if (articleCount != 0) {
            client.LabelNameCache.Edit(tag, false, false);
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.TagArticles(string tag, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).TagArticles(tag, articles.Select(article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForTag(tag), true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).UntagArticles(tag, articles.Select(article => article.ShortId), cancellationToken);

}