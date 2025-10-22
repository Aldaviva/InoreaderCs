using InoreaderCs.Auth;
using InoreaderCs.Entities;
using InoreaderCs.RateLimiting;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs;

/// <summary>
/// Client for the Inoreader HTTP API. To get started, construct a new <see cref="InoreaderClient"/>.
/// </summary>
/// <remarks>See <see href="https://www.inoreader.com/developers/"/></remarks>
public interface IInoreaderClient: IDisposable {

    /// <summary>
    /// The HTTP client that this library will use for requests to the Inoreader API. Can be set in the <see cref="InoreaderClient(IUserAuthToken,System.Net.Http.HttpClient?,bool?)"/> constructor. Will be a default instance if you didn't supply one to the constructor.
    /// </summary>
    HttpClient HttpClient { get; }

    // /// <summary>
    // /// An OAuth user access token or an app user access token, set in the <see cref="InoreaderClient(IUserAuthToken,System.Net.Http.HttpClient?,bool?)"/> constructor.
    // /// </summary>
    // IUserAuthToken AuthToken { get; }

    /// <summary>
    /// Fired after every API response so you can keep track of rate limits if you wish
    /// </summary>
    event EventHandler<RateLimitStatistics>? RateLimitStatisticsReceived;

    /// <summary>
    /// <para>List articles in a given feed, folder, tag, state, or other stream.</para>
    /// <para>For example, to list the 20 most recent unread articles, you can set <paramref name="stream"/> to <see cref="StreamId.ReadingList"/> and <paramref name="subtract"/> to <see cref="StreamId.Read"/>.</para>
    /// <para>To list starred articles, you can set <paramref name="stream"/> to <see cref="StreamId.ReadingList"/> and <paramref name="intersect"/> to <see cref="StreamId.Starred"/>.</para>
    /// <para>To list articles from feeds in the custom user folder "Science," you can set <paramref name="stream"/> to <c>StreamId.Label("Science")</c>.</para>
    /// </summary>
    /// <param name="stream">A feed (<see cref="StreamId.Feed"/>), tag/folder (<see cref="StreamId.Label"/>), state (like <see cref="StreamId.Starred"/>), or your entire newsfeed (<see cref="StreamId.ReadingList"/>) containing the articles you want to find.</param>
    /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
    /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="ascendingOrder"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
    /// <param name="subtract">Exclude articles which also appear in this stream, for example, <see cref="StreamId.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for appearing in any streams.</param>
    /// <param name="intersect">Only return articles which also appear in this stream, for example, <see cref="StreamId.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not appearing in any streams.</param>
    /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
    /// <param name="ascendingOrder"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
    /// <param name="includeFoldersInLabels"><c>false</c> for the response <see cref="Article.Labels"/> to only contain system labels like <see cref="StreamId.Starred"/> as well as tags you have added to the article, but not the folder that the article's feed is in, or <c>true</c> to also contain the folder.</param>
    /// <param name="includeAnnotations"><c>true</c> for the response <see cref="Article.Annotations"/> to be populated with the annotations you have added to the article, or <c>false</c> to leave it as the empty list.</param>
    /// <returns>Response envelope containing zero or more <see cref="Article"/> instances.</returns>
    /// <remarks>See <see href="https://www.inoreader.com/developers/stream-contents"/></remarks>
    /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
    Task<FullArticles> ListFullArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                        PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFoldersInLabels = true, bool includeAnnotations = false);

    /// <summary>
    /// <para>List only the IDs, crawl times, and labels of articles in a given feed, folder, tag, state, or other stream.</para>
    /// <para>This method is more lightweight than <see cref="ListFullArticles"/>, and should be used whenever it is sufficient, so that server load and bandwidth are minimized.</para>
    /// </summary>
    /// <param name="stream">A feed (<see cref="StreamId.Feed"/>), tag/folder (<see cref="StreamId.Label"/>), state (like <see cref="StreamId.Starred"/>), or your entire newsfeed (<see cref="StreamId.ReadingList"/>) containing the articles you want to find.</param>
    /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
    /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="ascendingOrder"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
    /// <param name="subtract">Exclude articles which also appear in this stream, for example, <see cref="StreamId.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for appearing in any streams.</param>
    /// <param name="intersect">Only return articles which also appear in this stream, for example, <see cref="StreamId.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not appearing in any streams.</param>
    /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
    /// <param name="ascendingOrder"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
    /// <param name="includeFoldersInLabels"><c>false</c> for the response <see cref="Article.Labels"/> to only contain system labels like <see cref="StreamId.Starred"/> as well as tags you have added to the article, but not the folder that the article's feed is in, or <c>true</c> to also contain the folder.</param>
    /// <returns>Response envelope containing zero or more <see cref="MinimalArticle"/> objects</returns>
    /// <remarks>See <see href="https://www.inoreader.com/developers/item-ids"/></remarks>
    /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
    Task<MinimalArticles> ListMinimalArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                              PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFoldersInLabels = true);

    /// <summary>
    /// <para>Add or remove a <paramref name="label"/> from one or more <paramref name="articles"/>.</para>
    /// <para>For example, to mark an article as read, you can set <paramref name="label"/> to <see cref="StreamId.Read"/> and <paramref name="removeLabel"/> to <c>false</c>.</para>
    /// <para>To unstar an article, you can set <paramref name="label"/> to <see cref="StreamId.Starred"/> and <paramref name="removeLabel"/> to <c>true</c>.</para>
    /// <para>To tag an article with the custom user tag "Important," you can set <paramref name="label"/> to <c>StreamId.Label("Important")</c> and <paramref name="removeLabel"/> to <c>false</c>.</para>
    /// </summary>
    /// <param name="label">Either a tag (that you created, like <see cref="StreamId.Label"/>) or system state label (like <see cref="StreamId.Read"/> or <see cref="StreamId.Starred"/>) to add or remove from the <paramref name="articles"/></param>
    /// <param name="removeLabel"><c>false</c> to add <paramref name="label"/> to all <paramref name="articles"/> if they don't already have it, or <c>true</c> to remove <paramref name="label"/> from all <paramref name="articles"/> if they have it</param>
    /// <param name="articles">one or more articles to modify</param>
    /// <remarks>See <see href="https://www.inoreader.com/developers/edit-tag"/></remarks>
    /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
    Task LabelArticles(StreamId label, bool removeLabel = false, params IEnumerable<Article> articles);

    /// <inheritdoc cref="LabelArticles(StreamId,bool,IEnumerable{Article})" />
    Task LabelArticles(StreamId label, bool removeLabel = false, params IEnumerable<string> articleIds);

}