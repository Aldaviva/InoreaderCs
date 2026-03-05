using System.Net;
using Unfucked.HTTP.Exceptions;

namespace Tests.Requests;

public class ExceptionsTest: ApiTest {

    [Fact]
    public async Task Unauthorized() {
        var request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=20&s=user/-/state/com.google/reading-list&includeAllDirectStreamIds=true"), null, string.Empty,
            HttpStatusCode.Unauthorized);

        await Inoreader.Newsfeed.Invoking(methods => methods.ListArticlesBrief())
            .Should().ThrowAsync<InoreaderException.Unauthorized>()
            .WithMessage("Inoreader auth failure")
            .WithInnerException<InoreaderException.Unauthorized, NotAuthorizedException>();

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TransformHttpExceptions() {
        Uri feedUrl = new("https://feeds.arstechnica.com/arstechnica/index");

        A.CallTo(() => HttpHandler.TestableSendAsync(A<HttpRequestMessage>._, A<CancellationToken>._)).ReturnsLazily(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        await Inoreader.Invoking(i => i.Newsfeed.ListArticlesDetailed()).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Articles.MarkArticles(ArticleState.Read, ["abc"])).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Newsfeed.MarkAllArticlesAsRead(DateTimeOffset.Now)).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Folders.Rename("a", "b")).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Folders.Delete("a")).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Folders.GetUnreadCounts()).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Subscriptions.Rename(feedUrl, "Ars Technica")).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Subscriptions.List()).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Subscriptions.Subscribe(feedUrl, CancellationToken.None)).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Subscriptions.ListArticlesBrief(feedUrl)).Should().ThrowAsync<InoreaderException>();
        await Inoreader.Invoking(i => i.Users.GetSelf()).Should().ThrowAsync<InoreaderException>();

    }

}