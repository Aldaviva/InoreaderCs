namespace Tests.Requests;

public class EditTagTest: ApiTest {

    [Fact]
    public async Task SendNoRequestIfZeroItemIds() {
        await Inoreader.Articles.Tag("My tag", Enumerable.Empty<string>(), CancellationToken.None);

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TagArticles() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "a=user%2F-%2Flabel%2FMy+tag&i=344691561&i=344668222", "OK");

        await Inoreader.Articles.Tag("My tag", articles: [
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
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UntagArticle() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "r=user%2F-%2Flabel%2FMy+tag&i=344691561", "OK");

        await Inoreader.Articles.Untag("My tag", articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkArticleRead() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "a=user%2F-%2Fstate%2Fcom.google%2Fread&i=344691561", "OK");

        await Inoreader.Articles.Mark(ArticleState.Read, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StarArticle() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "a=user%2F-%2Fstate%2Fcom.google%2Fstarred&i=344691561", "OK");

        await Inoreader.Articles.Mark(ArticleState.Starred, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkArticleUnread() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "r=user%2F-%2Fstate%2Fcom.google%2Fread&i=344691561", "OK");

        await Inoreader.Articles.Unmark(ArticleState.Read, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnstarArticle() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/edit-tag"), "r=user%2F-%2Fstate%2Fcom.google%2Fstarred&i=344691561", "OK");

        await Inoreader.Articles.Unmark(ArticleState.Starred, articles: [
            new Article {
                LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
                Title     = string.Empty,
                Author    = string.Empty,
                CrawlTime = DateTimeOffset.Now
            }
        ], TestContext.Current.CancellationToken);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}