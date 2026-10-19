using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Requests;

internal sealed partial class ClientRequests {

    /// <inheritdoc />
    public Task<Article?> Get(BriefArticle briefArticle, CancellationToken cancellationToken) => Get(briefArticle.ShortId, cancellationToken);

    /// <inheritdoc />
    public async Task<Article?> Get(string articleId, CancellationToken cancellationToken) {
        try {
            Task<LabelNameCache.Labels> labelNamesTask = client.LabelNameCache.GetLabelNames(false, cancellationToken);

            Article? article = (await ApiBase
                    .Path("stream/contents/article/{id}")
                    .ResolveTemplate("id", articleId)
                    .Get<DetailedArticles>(cancellationToken)
                    .ConfigureAwait(false))
                .Articles
                .SingleOrDefault();

            if (article is not null) {
                LabelNameCache.Labels labels = await labelNamesTask.ConfigureAwait(false);

                try {
                    article.SetCategories(labels);
                } catch (InoreaderException.UnknownLabel) {
                    // If the cache of label names and types is outdated and missing this label, this method gets exactly one free do-over to re-request the label names, uncached.
                    labels = await client.LabelNameCache.GetLabelNames(true, cancellationToken).ConfigureAwait(false);
                    article.SetCategories(labels);
                }
            }

            return article;
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to find an article with ID {articleId}");
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Mark(ArticleState markState, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForState(markState)!, false, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Mark(ArticleState markState, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).Mark(markState, articles.Select(static article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Unmark(ArticleState unmarkState, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForState(unmarkState)!, true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Unmark(ArticleState unmarkState, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).Unmark(unmarkState, articles.Select(static article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.IArticleMethods.Tag(string tag, IEnumerable<string> articleIds, CancellationToken cancellationToken) {
        int articleCount = await MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articleIds).ConfigureAwait(false);
        if (articleCount != 0) {
            client.LabelNameCache.Edit(tag, false, false);
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Tag(string tag, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).Tag(tag, articles.Select(static article => article.ShortId), cancellationToken);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Untag(string tag, IEnumerable<string> articleIds, CancellationToken cancellationToken) =>
        MarkArticles(StreamId.ForTag(tag), true, cancellationToken, articleIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.Untag(string tag, IEnumerable<Article> articles, CancellationToken cancellationToken) =>
        ((IInoreaderClient.IArticleMethods) this).Untag(tag, articles.Select(static article => article.ShortId), cancellationToken);

}