using System.Linq.Expressions;

namespace Tests.Requests;

public class EditTagTest: ApiTest {

    [Fact]
    public async Task SendNoRequestIfZeroItemIds() {
        await Inoreader.Articles.TagArticles("My tag", CancellationToken.None, Enumerable.Empty<string>());

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TagArticles() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "a=user%2F-%2Flabel%2FMy+tag&i=344691561&i=344668222", "OK");

        await Inoreader.Articles.TagArticles("My tag", articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            },
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b383e",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UntagArticle() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "r=user%2F-%2Flabel%2FMy+tag&i=344691561", "OK");

        await Inoreader.Articles.UntagArticles("My tag", articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkArticleRead() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "a=user%2F-%2Fstate%2Fcom.google%2Fread&i=344691561", "OK");

        await Inoreader.Articles.MarkArticles(ArticleState.Read, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StarArticle() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "a=user%2F-%2Fstate%2Fcom.google%2Fstarred&i=344691561", "OK");

        await Inoreader.Articles.MarkArticles(ArticleState.Starred, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkArticleUnread() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "r=user%2F-%2Fstate%2Fcom.google%2Fread&i=344691561", "OK");

        await Inoreader.Articles.UnmarkArticles(ArticleState.Read, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnstarArticle() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"),
            "r=user%2F-%2Fstate%2Fcom.google%2Fstarred&i=344691561", "OK");

        await Inoreader.Articles.UnmarkArticles(ArticleState.Starred, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}