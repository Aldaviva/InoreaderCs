using InoreaderCs.Entities;
using UnionTypes;

namespace InoreaderCs.Requests;

internal partial class Requests {

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.MarkArticles(ArticleState markState, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForState(markState)!, false, cancellationToken, articlesOrIds);

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForState(unmarkState)!, true, cancellationToken, articlesOrIds);

    /// <inheritdoc />
    async Task IInoreaderClient.IArticleMethods.TagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) {
        await MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articlesOrIds).ConfigureAwait(false);
        client.LabelNameCache.Edit(tag, false, false);
    }

    /// <inheritdoc />
    Task IInoreaderClient.IArticleMethods.UntagArticles(string tag, CancellationToken cancellationToken, params IEnumerable<Union<Article, string>> articlesOrIds) =>
        MarkArticles(StreamId.ForTag(tag), false, cancellationToken, articlesOrIds);

}