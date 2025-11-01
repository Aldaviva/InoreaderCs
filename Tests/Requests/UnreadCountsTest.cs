using System.Linq.Expressions;

namespace Tests.Requests;

public class UnreadCountsTest: ApiTest {

    //language=json
    private const string UnreadCountResponseJson =
        """
        { "max": 1000, "unreadcounts": [{ "id": "user\/1006195123\/state\/com.google\/reading-list", "count": 917, "newestItemTimestampUsec": "1761974703493932" }, { "id": "user\/1006195123\/state\/com.google\/starred", "count": "1000", "newestItemTimestampUsec": "0" }, { "id": "user\/1006195123\/label\/Comics", "count": 3, "newestItemTimestampUsec": "0" }, { "id": "user\/1006195123\/label\/Telco news", "count": 1, "newestItemTimestampUsec": "0" }, { "id": "feed\/https:\/\/feeds.arstechnica.com\/arstechnica\/index", "count": 7, "newestItemTimestampUsec": "1761961046125286" }] }
        """;

    //language=json
    private const string StreamTypesResponseJson =
        """
        { "tags": [{ "id": "user\/1006195123\/state\/com.google\/starred", "sortid": "FFFFFFFF" }, { "id": "user\/1006195123\/state\/com.google\/broadcast", "sortid": "FFFFFFFE" }, { "id": "user\/1006195123\/state\/com.google\/blogger-following", "sortid": "FFFFFFFD" }, { "id": "user\/1006195123\/label\/Comics", "sortid": "BF3834D6", "unread_count": 3, "unseen_count": 1, "type": "folder", "pinned": 1, "article_count": 0, "article_count_today": 0 }, { "id": "user\/1006195123\/label\/Telco news", "sortid": "BF844B22", "unread_count": 1, "unseen_count": 0, "type": "tag", "pinned": 1, "article_count": 9, "article_count_today": 2 }] }
        """;

    [Fact]
    public async Task GetNewsfeedUnreadCounts() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/unread-count"), null, UnreadCountResponseJson);

        NewsfeedUnreadCounts actual = await Inoreader.Newsfeed.GetUnreadCounts();

        actual.AllArticles.UnreadCount.Should().Be(917);
        actual.AllArticles.NewestArticleTime.Should().Be(new DateTimeOffset(2025, 10, 31, 22, 25, 03, 493, 932, TimeSpan.FromHours(-7)));
        actual.StarredArticleCount.Should().Be(1000);
        actual.MaxDisplayableCount.Should().Be(1000);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetFolderUnreadCounts() {
        Expression<Func<Task<HttpResponseMessage>>> unreadCountRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/unread-count"), null, UnreadCountResponseJson);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        LabelUnreadCounts unreadCounts = await Inoreader.Folders.GetUnreadCounts();
        unreadCounts.MaxDisplayableCount.Should().Be(1000);
        unreadCounts.UnreadCountsByLabelName.Should().HaveCount(1);
        unreadCounts.UnreadCountsByLabelName["Comics"].UnreadCount.Should().Be(3);
        unreadCounts.UnreadCountsByLabelName["Comics"].NewestArticleTime.Should().BeNull();

        A.CallTo(unreadCountRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetTagUnreadCounts() {
        Expression<Func<Task<HttpResponseMessage>>> unreadCountRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/unread-count"), null, UnreadCountResponseJson);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        LabelUnreadCounts unreadCounts = await Inoreader.Tags.GetUnreadCounts();
        unreadCounts.MaxDisplayableCount.Should().Be(1000);
        unreadCounts.UnreadCountsByLabelName.Should().HaveCount(1);
        unreadCounts.UnreadCountsByLabelName["Telco news"].UnreadCount.Should().Be(1);
        unreadCounts.UnreadCountsByLabelName["Telco news"].NewestArticleTime.Should().BeNull();

        A.CallTo(unreadCountRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionUnreadCounts() {
        Expression<Func<Task<HttpResponseMessage>>> unreadCountRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/unread-count"), null, UnreadCountResponseJson);

        SubscriptionUnreadCounts unreadCounts = await Inoreader.Subscriptions.GetUnreadCounts();
        unreadCounts.MaxDisplayableCount.Should().Be(1000);
        Uri feedUri = new("https://feeds.arstechnica.com/arstechnica/index");
        unreadCounts.Subscriptions.Should().HaveCount(1);
        unreadCounts.Subscriptions[feedUri].UnreadCount.Should().Be(7);
        unreadCounts.Subscriptions[feedUri].NewestArticleTime.Should().Be(new DateTimeOffset(2025, 10, 31, 18, 37, 26, 125, 286, TimeSpan.FromHours(-7)));

        A.CallTo(unreadCountRequest).MustHaveHappenedOnceExactly();
    }

}