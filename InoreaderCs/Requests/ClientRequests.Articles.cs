using InoreaderCs.Entities;

namespace InoreaderCs.Requests;

internal partial class ClientRequests {

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, CancellationToken cancellationToken, params IEnumerable<string> articleIds) =>
        MarkArticles(StreamId.ForState(markState)!, false, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, CancellationToken cancellationToken, params IEnumerable<Article> articles) =>
        ((IInoreaderClient.IArticleMethods) this).MarkArticles(markState, cancellationToken, articles.Select(article => article.ShortId));

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken, params IEnumerable<string> articleIds) =>
        MarkArticles(StreamId.ForState(unmarkState)!, true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken, params IEnumerable<Article> articles) =>
        ((IInoreaderClient.IArticleMethods) this).UnmarkArticles(unmarkState, cancellationToken, articles.Select(article => article.ShortId));

    /// <inheritdoc />
    async Task IInoreaderClient.IArticleMethods.TagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<string> articleIds) {
        int articleCount = await MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articleIds).ConfigureAwait(false);
        if (articleCount != 0) {
            client.LabelNameCache.Edit(tag, false, false);
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.TagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Article> articles) =>
        ((IInoreaderClient.IArticleMethods) this).TagArticles(tag, cancellationToken, articles.Select(article => article.ShortId));

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<string> articleIds) =>
        MarkArticles(StreamId.ForTag(tag), true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Article> articles) =>
        ((IInoreaderClient.IArticleMethods) this).UntagArticles(tag, cancellationToken, articles.Select(article => article.ShortId));

}