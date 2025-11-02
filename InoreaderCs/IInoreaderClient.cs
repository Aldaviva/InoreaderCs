using InoreaderCs.Entities;
using InoreaderCs.RateLimit;
using Unfucked.HTTP.Config;
using Unfucked.HTTP.Exceptions;
using Unfucked.HTTP.Filters;

namespace InoreaderCs;

/// <summary>
/// Client for the Inoreader HTTP API. To get started, construct a new <see cref="InoreaderClient"/>.
/// </summary>
/// <seealso href="https://www.inoreader.com/developers" />
public interface IInoreaderClient: IDisposable {

    // /// <summary>
    // /// The HTTP client that this library will use for requests to the Inoreader API. Can be set in the <see cref="InoreaderClient(IAuthClient,IUnfuckedHttpClient?,bool?)"/> constructor. Will be a default instance if you didn't supply one to the constructor.
    // /// </summary>
    // IUnfuckedHttpClient HttpClient { get; }

    /// <summary>
    /// <para>Target for Inoreader API with preconfigured URL (<c>https://www.inoreader.com/reader/api/0/user-info</c>), content type, authentication, rate-limit metrics, and JSON deserialization settings.</para>
    /// <para>To add your own request or response filtering, register a <see cref="ClientRequestFilter"/> or <see cref="ClientResponseFilter"/> on this using <see cref="Configurable{TContainer}.Register"/>, then set this <see cref="ApiBase"/> property to the new immutable copy that <c>Register</c> returns (it's immutable, so if you don't set the new value the change will be ignored).</para>
    /// </summary>
    WebTarget ApiBase { get; set; }

    /// <summary>
    /// Fired after every API response so you can keep track of rate limit quota usage if you wish.
    /// </summary>
    event EventHandler<RateLimitStatistics>? RateLimitStatisticsReceived;

    /// <summary>
    /// API methods to list, update, and delete folders. Also lets you list, count unread, and mark as read those articles in a folder.
    /// </summary>
    public interface IFolderMethods {

        /// <summary>
        /// Get all folders of feed subscriptions that the user has created.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>A list of zero or more folders.</returns>
        /// <seealso href="https://www.inoreader.com/developers/tag-list"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<IEnumerable<FolderState>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List articles in a given folder.</para>
        /// <para>For example, to list the 20 most recent unread articles, you can set <paramref name="subtract"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To list starred articles, you can set <paramref name="intersect"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If you only need the IDs, timestamps, tags, or folders of the articles rather than all of their contents, you can save bandwidth by calling <see cref="ListArticlesBrief"/> instead.</para>
        /// </summary>
        /// <param name="inFolder">Name of the folder to list articles from, such as <c>My Folder</c>.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="showAnnotations"><c>true</c> for the response <see cref="Article.Annotations"/> to be populated with the annotations you have added to the article, or <c>false</c> to leave it as the empty list.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="Article"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/stream-contents" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<DetailedArticles> ListArticlesDetailed(string inFolder, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                                    PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, bool showAnnotations = false,
                                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List only the IDs, crawl times, and labels of articles in a given folder.</para>
        /// <para>This method is more lightweight than <see cref="ListArticlesDetailed"/>, and should be used whenever it is sufficient, so that server load and bandwidth are minimized.</para>
        /// </summary>
        /// <param name="inFolder">Name of the folder to list articles from, such as <c>My Folder</c>.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 1000.</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="BriefArticle"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/item-ids" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<BriefArticles> ListArticlesBrief(string inFolder, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                              PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Count how many articles are unread in each folder.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>A mapping from each folder name to the number of unread articles and the timestamp of the latest article.</returns>
        /// <seealso href="https://www.inoreader.com/developers/unread-counts" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<LabelUnreadCounts> GetUnreadCounts(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark all articles in a folder as read.
        /// </summary>
        /// <param name="inFolder">Name of the folder to mark articles as read in, such as <c>My Folder</c>.</param>
        /// <param name="maxSeenArticleTime">The time when the article list was most recently fetched and rendered. This prevents very recent articles from being marked as read before the user can see them, if they were polled between when the feed was last rendered and when the mark as read API call is received by the server.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/mark-all-as-read"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkAllArticlesAsRead(string inFolder, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// Change the name of an existing folder.
        /// </summary>
        /// <param name="folder">The old name of the folder to rename, such as <c>My Folder</c>.</param>
        /// <param name="newName">The new name to set the folder to, such as <c>My Folder!</c>.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/rename-tag"/>
        /// <exception cref="ArgumentException"><paramref name="newName"/> contains a forward slash ("/").</exception>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Rename(string folder, string newName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove an existing folder.
        /// </summary>
        /// <param name="folder">Name of the folder to delete, such as <c>My Folder</c>.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/delete-tag"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Delete(string folder, CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// API methods to list, update, and delete tags. Also lets you list, count unread, and mark as read those articles with a tag.
    /// </summary>
    public interface ITagMethods {

        /// <summary>
        /// Get all tags that the user has created, as well as the quantities of unread and unseen articles in each tag.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>A list of zero or more tags.</returns>
        /// <seealso href="https://www.inoreader.com/developers/tag-list"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<IEnumerable<TagState>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List articles with a given tag.</para>
        /// <para>For example, to list the 20 most recent unread articles, you can set <paramref name="subtract"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To list starred articles, you can set <paramref name="intersect"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If you only need the IDs, timestamps, tags, or folders of the articles rather than all of their contents, you can save bandwidth by calling <see cref="ListArticlesBrief"/> instead.</para>
        /// </summary>
        /// <param name="withTag">Name of the tag to list articles from, such as <c>My Tag</c>.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="showAnnotations"><c>true</c> for the response <see cref="Article.Annotations"/> to be populated with the annotations you have added to the article, or <c>false</c> to leave it as the empty list.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="Article"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/stream-contents" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<DetailedArticles> ListArticlesDetailed(string withTag, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                                    PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, bool showAnnotations = false,
                                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List only the IDs, crawl times, and labels of articles with a given tag.</para>
        /// <para>This method is more lightweight than <see cref="ListArticlesDetailed"/>, and should be used whenever it is sufficient, so that server load and bandwidth are minimized.</para>
        /// </summary>
        /// <param name="withTag">Name of the tag to list articles from, such as <c>My Tag</c>.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 1000.</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="BriefArticle"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/item-ids" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<BriefArticles> ListArticlesBrief(string withTag, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                              PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Count how many articles are unread in each tag.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>A mapping from each tag name to the number of unread articles and the timestamp of the latest article.</returns>
        /// <seealso href="https://www.inoreader.com/developers/unread-counts" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<LabelUnreadCounts> GetUnreadCounts(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark all articles with a tag as read.
        /// </summary>
        /// <param name="withTag">Name of the tag to mark articles as read from, such as <c>My Tag</c>.</param>
        /// <param name="maxSeenArticleTime">The time when the article list was most recently fetched and rendered. This prevents very recent articles from being marked as read before the user can see them, if they were polled between when the feed was last rendered and when the mark as read API call is received by the server.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/mark-all-as-read"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkAllArticlesAsRead(string withTag, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// Change the name of an existing tag.
        /// </summary>
        /// <param name="tag">The old name of the tag to rename, such as <c>My Tag</c>.</param>
        /// <param name="newName">The new name to set the tag to, such as <c>My Tag!</c>.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/rename-tag"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Rename(string tag, string newName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove an existing tag.
        /// </summary>
        /// <param name="tag">Name of the tag to delete, such as <c>My Tag</c>.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/delete-tag"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Delete(string tag, CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// API methods to list, count unread, and mark all articles as read all the articles in the user's entire newsfeed/reading list/account.
    /// </summary>
    public interface INewsfeedMethods {

        /// <summary>
        /// <para>List articles in the user's entire newsfeed.</para>
        /// <para>For example, to list the 20 most recent unread articles, you can set <paramref name="subtract"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To list starred articles, you can set <paramref name="intersect"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If you only need the IDs, timestamps, tags, or folders of the articles rather than all of their contents, you can save bandwidth by calling <see cref="ListArticlesBrief"/> instead.</para>
        /// </summary>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="showAnnotations"><c>true</c> for the response <see cref="Article.Annotations"/> to be populated with the annotations you have added to the article, or <c>false</c> to leave it as the empty list.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="Article"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/stream-contents" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<DetailedArticles> ListArticlesDetailed(int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                                    PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, bool showAnnotations = false,
                                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List only the IDs, crawl times, and labels of articles in the user's entire newsfeed.</para>
        /// <para>This method is more lightweight than <see cref="ListArticlesDetailed"/>, and should be used whenever it is sufficient, so that server load and bandwidth are minimized.</para>
        /// </summary>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 1000.</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="BriefArticle"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/item-ids" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<BriefArticles> ListArticlesBrief(int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null, PaginationToken? pagination = null,
                                              bool sortAscending = false, bool showFolders = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Count how many articles are unread in the entire newsfeed. Also returns the quantity of articles that are both unread and starred in the newsfeed.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>The number of unread articles and the timestamp of the latest article for both the entire newsfeed and starred/favorited/saved for later articles.</returns>
        /// <seealso href="https://www.inoreader.com/developers/unread-counts" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<NewsfeedUnreadCounts> GetUnreadCounts(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark all articles in the user's entire newsfeed as read.
        /// </summary>
        /// <param name="maxSeenArticleTime">The time when the article list was most recently fetched and rendered. This prevents very recent articles from being marked as read before the user can see them, if they were polled between when the feed was last rendered and when the mark as read API call is received by the server.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/mark-all-as-read"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkAllArticlesAsRead(DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// API methods to subscribe to or unsubscribe from a feed, list or rename subscriptions, or add or remove subscriptions to or from folders.
    /// </summary>
    public interface ISubscriptionMethods {

        /// <summary>
        /// Get all feeds to which the user is subscribed.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>List of feed subscriptions in an undefined order.</returns>
        /// <seealso href="https://www.inoreader.com/developers/subscription-list"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<IEnumerable<Subscription>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List articles in a given feed subscription.</para>
        /// <para>For example, to list the 20 most recent unread articles, you can set <paramref name="subtract"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To list starred articles, you can set <paramref name="intersect"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If you only need the IDs, timestamps, tags, or folders of the articles rather than all of their contents, you can save bandwidth by calling <see cref="ListArticlesBrief"/> instead.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 200 (not 100 as documented).</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="showAnnotations"><c>true</c> for the response <see cref="Article.Annotations"/> to be populated with the annotations you have added to the article, or <c>false</c> to leave it as the empty list.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="Article"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/stream-contents" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<DetailedArticles> ListArticlesDetailed(Uri feedLocation, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                                    PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, bool showAnnotations = false,
                                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>List only the IDs, crawl times, and labels of articles in a given feed subscription.</para>
        /// <para>This method is more lightweight than <see cref="ListArticlesDetailed"/>, and should be used whenever it is sufficient, so that server load and bandwidth are minimized.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="maxArticles">Maximum number of articles to return in one page, limited to 1000.</param>
        /// <param name="minTime">Lower bound on the <see cref="BaseArticle.CrawlTime"/> of articles to return, or no limit if omitted. When <paramref name="sortAscending"/> is <c>true</c>, this is limited to at most 30 days ago.</param>
        /// <param name="subtract">Exclude articles which also have this state, for example, <see cref="ArticleState.Read"/> to only return unread articles, or <c>null</c> to not exclude any articles for having any state.</param>
        /// <param name="intersect">Only return articles which also have this state, for example, <see cref="ArticleState.Starred"/> to only return starred articles, or <c>null</c> to not exclude any articles for not having any state.</param>
        /// <param name="pagination">The <see cref="PaginatedListResponse.PaginationToken"/> from a prior response to fetch subsequent pages, or <c>null</c> to fetch the first page.</param>
        /// <param name="sortAscending"><c>true</c> to sort articles in the response list ascending by <see cref="BaseArticle.CrawlTime"/>, or <c>false</c> to sort them descending.</param>
        /// <param name="showFolders"><c>true</c> for the response <see cref="Article.Folders"/> to be populated with the folders the article feed's subscription has been organized into, or <c>false</c> for it to be the empty set.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>Response envelope containing zero or more <see cref="BriefArticle"/> objects.</returns>
        /// <seealso href="https://www.inoreader.com/developers/item-ids" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<BriefArticles> ListArticlesBrief(Uri feedLocation, int maxArticles = 20, DateTimeOffset? minTime = null, ArticleState? subtract = null, ArticleState? intersect = null,
                                              PaginationToken? pagination = null, bool sortAscending = false, bool showFolders = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Rename an existing feed subscription.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="newTitle">Custom name to give the subscription.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-subscription"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Rename(Uri feedLocation, string newTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Add an existing feed subscription to a folder.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="folder">Name of a folder to move the subscription into. A subscription can be contained in multiple folders, so adding it to one folder does not remove it from any folders it was already in. If the folder does not already exist, it will be created automatically.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-subscription"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task AddToFolder(Uri feedLocation, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Remove a feed subscription from a folder.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="folder">Remove the subscription from the folder with this name. If the subscription is no longer in any folders after removing it from this folder, it will appear in the top level, outside of any folders.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-subscription"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task RemoveFromFolder(Uri feedLocation, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Follow a feed by subscribing to it.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>The new subscription and whether it was successfully added or not. If the feed was a duplicate which was ignored, this will also be considered successful.</returns>
        /// <seealso href="https://www.inoreader.com/developers/add-subscription" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<SubscriptionCreationResult> Subscribe(Uri feedLocation, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Follow a feed by subscribing to it.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="title">Custom name to give the new subscription instead of the feed's default name from its RSS channel <c>&lt;title&gt;</c>, or <c>null</c> to use that default.</param>
        /// <param name="folder">Name of a folder to add this subscription to in Inoreader, or <c>null</c> to add it to the top level, outside of any folders.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>The new subscription and whether it was successfully added or not. If the feed was a duplicate which was ignored, this will also be considered successful.</returns>
        /// <seealso href="https://www.inoreader.com/developers/edit-subscription"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Subscribe(Uri feedLocation, string? title = null, string? folder = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// <para>Unsubscribe from or unfollow a feed, deleting its articles from your newsfeed.</para>
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-subscription"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task Unsubscribe(Uri feedLocation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Count how many articles are unread for each feed subscription.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>A mapping from each subscription feed URL to the number of unread articles and the timestamp of the latest article.</returns>
        /// <seealso href="https://www.inoreader.com/developers/unread-counts" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<SubscriptionUnreadCounts> GetUnreadCounts(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark all articles in one feed subscription as read.
        /// </summary>
        /// <param name="feedLocation">URL of the RSS or Atom XML document for the feed.</param>
        /// <param name="maxSeenArticleTime">The time when the article list was most recently fetched and rendered. This prevents very recent articles from being marked as read before the user can see them, if they were polled between when the feed was last rendered and when the mark as read API call is received by the server.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/mark-all-as-read"/>
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkAllArticlesAsRead(Uri feedLocation, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken = default);

    }

    /// <summary>
    /// API methods to mark or unmark articles with system state, such as starred or read, as well as add or remove tags to or from articles.
    /// </summary>
    public interface IArticleMethods {

        /// <summary>
        /// <para>Add an <see cref="ArticleState"/> to one or more <paramref name="articles"/>.</para>
        /// <para>For example, to mark an article as read, you can set <paramref name="markState"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To star an article, you can set <paramref name="markState"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If an article already has the specified <paramref name="markState"/>, it is preserve and ignored, instead of throwing an error.</para>
        /// <para>To tag an article, see <see cref="TagArticles(string,CancellationToken,IEnumerable{Article})"/>.</para>
        /// </summary>
        /// <param name="markState">A system state, like <see cref="ArticleState.Read"/> or <see cref="ArticleState.Starred"/>, to add to the <paramref name="articles"/>.</param>
        /// <param name="articles">One or more <see cref="Article"/>s to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkArticles(ArticleState markState, CancellationToken cancellationToken = default, params IEnumerable<Article> articles);

        /// <summary>
        /// <para>Add an <see cref="ArticleState"/> to one or more <paramref name="articlesIds"/>.</para>
        /// <para>For example, to mark an article as read, you can set <paramref name="markState"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To star an article, you can set <paramref name="markState"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If an article already has the specified <paramref name="markState"/>, it is preserve and ignored, instead of throwing an error.</para>
        /// <para>To tag an article, see <see cref="TagArticles(string,CancellationToken,IEnumerable{string})"/>.</para>
        /// </summary>
        /// <param name="markState">A system state, like <see cref="ArticleState.Read"/> or <see cref="ArticleState.Starred"/>, to add to the <paramref name="articlesIds"/>.</param>
        /// <param name="articlesIds">One or more <see cref="Article"/> IDs (<see cref="Article.ShortId"/> or <see cref="Article.LongId"/>) to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task MarkArticles(ArticleState markState, CancellationToken cancellationToken = default, params IEnumerable<string> articlesIds);

        /// <summary>
        /// <para>Remove an <see cref="ArticleState"/> from one or more <paramref name="articles"/>.</para>
        /// <para>For example, to mark an article as unread, you can set <paramref name="unmarkState"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To unstar an article, you can set <paramref name="unmarkState"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If an article already doesn't have the specified <paramref name="unmarkState"/>, it is ignored, instead of throwing an error.</para>
        /// <para>To untag an article, see <see cref="UntagArticles(string,CancellationToken,IEnumerable{Article})"/>.</para>
        /// </summary>
        /// <param name="unmarkState">A system state, like <see cref="ArticleState.Read"/> or <see cref="ArticleState.Starred"/>, to remove from each article.</param>
        /// <param name="articles">One or more <see cref="Article"/>s to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken = default, params IEnumerable<Article> articles);

        /// <summary>
        /// <para>Remove an <see cref="ArticleState"/> from one or more <paramref name="articlesIds"/>.</para>
        /// <para>For example, to mark an article as unread, you can set <paramref name="unmarkState"/> to <see cref="ArticleState.Read"/>.</para>
        /// <para>To unstar an article, you can set <paramref name="unmarkState"/> to <see cref="ArticleState.Starred"/>.</para>
        /// <para>If an article already doesn't have the specified <paramref name="unmarkState"/>, it is ignored, instead of throwing an error.</para>
        /// <para>To untag an article, see <see cref="UntagArticles(string,CancellationToken,IEnumerable{string})"/>.</para>
        /// </summary>
        /// <param name="unmarkState">A system state, like <see cref="ArticleState.Read"/> or <see cref="ArticleState.Starred"/>, to remove from each article.</param>
        /// <param name="articlesIds">One or more <see cref="Article"/> IDs (<see cref="Article.ShortId"/> or <see cref="Article.LongId"/>) to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task UnmarkArticles(ArticleState unmarkState, CancellationToken cancellationToken = default, params IEnumerable<string> articlesIds);

        /// <summary>
        /// <para>Add a <paramref name="tag"/> to one or more <paramref name="articles"/>.</para>
        /// <para>For example, to tag an article with the custom user tag "Important," you can set <paramref name="tag"/> to <c>Important</c>.</para>
        /// <para>If an article already has the specified <paramref name="tag"/>, it is preserve and ignored, instead of throwing an error.</para>
        /// <para>To mark an article as starred or read, see <see cref="MarkArticles(ArticleState,CancellationToken,IEnumerable{Article})"/>.</para>
        /// </summary>
        /// <param name="tag">The name of a custom user tag, such as <c>Important</c>, to add to each article.</param>
        /// <param name="articles">One or more <see cref="Article"/>s to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task TagArticles(string tag, CancellationToken cancellationToken = default, params IEnumerable<Article> articles);

        /// <summary>
        /// <para>Add a <paramref name="tag"/> to one or more <paramref name="articleIds"/>.</para>
        /// <para>For example, to tag an article with the custom user tag "Important," you can set <paramref name="tag"/> to <c>Important</c>.</para>
        /// <para>If an article already has the specified <paramref name="tag"/>, it is preserve and ignored, instead of throwing an error.</para>
        /// <para>To mark an article as starred or read, see <see cref="MarkArticles(ArticleState,CancellationToken,IEnumerable{string})"/>.</para>
        /// </summary>
        /// <param name="tag">The name of a custom user tag, such as <c>Important</c>, to add to each article.</param>
        /// <param name="articleIds">One or more <see cref="Article"/> IDs (<see cref="Article.ShortId"/> or <see cref="Article.LongId"/>) to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task TagArticles(string tag, CancellationToken cancellationToken = default, params IEnumerable<string> articleIds);

        /// <summary>
        /// <para>Remove a tag from one or more <paramref name="articles"/>.</para>
        /// <para>For example, to untag an article with the custom user tag "Important," you can set <paramref name="tag"/> to <c>Important</c>.</para>
        /// <para>If an article already doesn't have the specified <paramref name="tag"/>, it is ignored, instead of throwing an error.</para>
        /// <para>To unmark an article as starred or read, see <see cref="UnmarkArticles(ArticleState,CancellationToken,IEnumerable{Article})"/>.</para>
        /// </summary>
        /// <param name="tag">The name of a custom user tag, such as <c>Important</c>, to remove from each article.</param>
        /// <param name="articles">One or more <see cref="Article"/>s to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task UntagArticles(string tag, CancellationToken cancellationToken = default, params IEnumerable<Article> articles);

        /// <summary>
        /// <para>Remove a tag from one or more <paramref name="articleIds"/>.</para>
        /// <para>For example, to untag an article with the custom user tag "Important," you can set <paramref name="tag"/> to <c>Important</c>.</para>
        /// <para>If an article already doesn't have the specified <paramref name="tag"/>, it is ignored, instead of throwing an error.</para>
        /// <para>To unmark an article as starred or read, see <see cref="UnmarkArticles(ArticleState,CancellationToken,IEnumerable{string})"/>.</para>
        /// </summary>
        /// <param name="tag">The name of a custom user tag, such as <c>Important</c>, to remove from each article.</param>
        /// <param name="articleIds">One or more <see cref="Article"/> IDs (<see cref="Article.ShortId"/> or <see cref="Article.LongId"/>) to modify.</param>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <seealso href="https://www.inoreader.com/developers/edit-tag" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task UntagArticles(string tag, CancellationToken cancellationToken = default, params IEnumerable<string> articleIds);

    }

    /// <summary>
    /// API method to get information about the user.
    /// </summary>
    public interface IUserMethods {

        /// <summary>
        /// Get information about the currently authenticated Inoreader user, including their name, ID, and email address.
        /// </summary>
        /// <param name="cancellationToken">To optionally abort the request before it finishes.</param>
        /// <returns>User information</returns>
        /// <seealso href="https://www.inoreader.com/developers/user-info" />
        /// <exception cref="InoreaderException">If an error occurred communicating with the Inoreader API, with subclasses (like <see cref="InoreaderException.Unauthorized"/>) for specific errors, and an inner <see cref="HttpException"/> for details.</exception>
        Task<User> GetSelf(CancellationToken cancellationToken = default);

    }

    /// <inheritdoc cref="IFolderMethods" />
    public IFolderMethods Folders { get; }

    /// <inheritdoc cref="ITagMethods" />
    public ITagMethods Tags { get; }

    /// <inheritdoc cref="INewsfeedMethods" />
    public INewsfeedMethods Newsfeed { get; }

    /// <inheritdoc cref="ISubscriptionMethods" />
    public ISubscriptionMethods Subscriptions { get; }

    /// <inheritdoc cref="IUserMethods" />
    public IUserMethods Users { get; }

    /// <inheritdoc cref="IArticleMethods" />
    public IArticleMethods Articles { get; }

}