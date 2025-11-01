using System.Linq.Expressions;

namespace Tests.Requests;

public class FeedsTest: ApiTest {

    //language=json
    private const string FeedListResponse =
        """
        { "subscriptions": [{ "id": "feed\/https:\/\/feeds.arstechnica.com\/arstechnica\/index", "feedType": "rss", "title": "Ars Technica", "categories": [{ "id": "user\/1006195123\/label\/Technology", "label": "Technology" }], "sortid": "05BDC790", "firstitemmsec": 1668623672220200, "url": "https:\/\/feeds.arstechnica.com\/arstechnica\/index", "htmlUrl": "https:\/\/arstechnica.com\/", "iconUrl": "https:\/\/www.inoreader.com\/fetch_icon\/arstechnica.com?w=16\u0026cs=2263185962\u0026v=1" }] }
        """;

    [Fact]
    public async Task FeedsList() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/subscription/list"), null, FeedListResponse);

        IEnumerable<Subscription> actual = await Inoreader.Subscriptions.List();

        Subscription head = actual.Should().ContainSingle().Subject;
        head.Title.Should().Be("Ars Technica");
        head.FeedUrl.Should().Be(new Uri("https://feeds.arstechnica.com/arstechnica/index"));
        head.PageUrl.Should().Be(new Uri("https://arstechnica.com/"));
        head.FaviconUrl.Should().Be(new Uri("https://www.inoreader.com/fetch_icon/arstechnica.com?w=16&cs=2263185962&v=1"));
        head.Folders.Should().ContainSingle().Which.Should().Be("Technology");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}