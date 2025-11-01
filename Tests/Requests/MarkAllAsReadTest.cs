using System.Linq.Expressions;

namespace Tests.Requests;

public class MarkAllAsReadTest: ApiTest {

    [Fact]
    public async Task MarkAllAsReadInNewsfeed() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/mark-all-as-read"),
            "s=user%2F-%2Fstate%2Fcom.google%2Freading-list&ts=946684800000000", "OK");

        await Inoreader.Newsfeed.MarkAllArticlesAsRead(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkAllAsReadInTag() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/mark-all-as-read"),
            "s=user%2F-%2Flabel%2FMy+tag&ts=946684800000000", "OK");

        await Inoreader.Tags.MarkAllArticlesAsRead("My tag", new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkAllAsReadInFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/mark-all-as-read"),
            "s=user%2F-%2Flabel%2FMy+folder&ts=946684800000000", "OK");

        await Inoreader.Folders.MarkAllArticlesAsRead("My folder", new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task MarkAllAsReadInSubscription() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/mark-all-as-read"),
            "s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F&ts=946684800000000", "OK");

        await Inoreader.Subscriptions.MarkAllArticlesAsRead(new Uri("https://arstechnica.com/science/feed/"), new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}