using System.Net.Mime;
using System.Text;
using Tests.Requests;

namespace Tests.Entities;

public class LabelNameCacheTest: ApiTest {

    [Fact]
    public async Task RefreshOnceOnCacheMiss() {
        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(req =>
            req.RequestUri == new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1")), A<CancellationToken>._)).ReturnsNextFromSequence(
            new HttpResponseMessage {
                Content = new StringContent( /*language=json*/ """
                    {
                        "tags": [
                            { "id": "user/-/label/Folder1", "type": "folder", "unread_count": 0, "unseen_count": 0 },
                            { "id": "user/-/label/Tag1", "type": "tag", "unread_count": 0, "unseen_count": 0, "pinned": 0, "article_count": 0, "article_count_today": 0 }
                        ]
                    }
                    """, Encoding.UTF8, MediaTypeNames.Application.Json)
            }, new HttpResponseMessage {
                Content = new StringContent( /*language=json*/ """
                    {
                        "tags": [
                            { "id": "user/-/label/Folder1", "type": "folder", "unread_count": 0, "unseen_count": 0 },
                            { "id": "user/-/label/Folder2", "type": "folder", "unread_count": 0, "unseen_count": 0 },
                            { "id": "user/-/label/Tag1", "type": "tag", "unread_count": 0, "unseen_count": 0, "pinned": 0, "article_count": 0, "article_count_today": 0 },
                            { "id": "user/-/label/Tag2", "type": "tag", "unread_count": 0, "unseen_count": 0, "pinned": 0, "article_count": 0, "article_count_today": 0 }
                        ]
                    }
                    """, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/contents/user%2F-%2Fstate%2Fcom.google%2Freading-list?n=20&includeAllDirectStreamIds=true&annotations=0"), null, """
            {
                "id": "user/1006195123/state/com.google/reading-list",
                "title": "Reading List",
                "updatedUsec": "1761986767029232",
                "items": [
                    {
                        "crawlTimeMsec": "1760450487167",
                        "timestampUsec": "1760450487166567",
                        "id": "tag:google.com,2005:reader\/item\/0000000ae763aaab",
                        "title": "Article 1",
                        "author": "Author",
                        "categories": ["user\/1006195123\/label\/Folder2"]
                    }
                ]
            }
            """);

        DetailedArticles articles = await Inoreader.Newsfeed.ListArticlesDetailed(cancellationToken: TestContext.Current.CancellationToken);

        Article article = articles.Articles[0];
        article.Folders.Should().Equal("Folder2");
        article.Tags.Should().BeEmpty();

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(req =>
            req.RequestUri == new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1")), A<CancellationToken>._)).MustHaveHappenedTwiceExactly();

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(req =>
                req.RequestUri == new Uri("https://www.inoreader.com/reader/api/0/stream/contents/user%2F-%2Fstate%2Fcom.google%2Freading-list?n=20&includeAllDirectStreamIds=true&annotations=0")),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

}