using System.Linq.Expressions;

namespace Tests.Requests;

public class SubscriptionsTest: ApiTest {

    [Fact]
    public async Task QuickAdd() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/quickadd"),
            expectedRequestBody: "quickadd=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F", jsonResponse:
            """{ "query": "https:\/\/arstechnica.com\/science\/feed\/", "numResults": 1, "streamId": "feed\/https:\/\/arstechnica.com\/science\/feed\/", "streamName": "Ars Technica \u0026raquo; Scientific Method" }""");

        SubscriptionCreationResult actual = await Inoreader.Subscriptions.Subscribe(new Uri("https://arstechnica.com/science/feed/"), CancellationToken.None);

        actual.IsSuccesfullySubscribed.Should().BeTrue();
        actual.FeedName.Should().Be("Ars Technica &raquo; Scientific Method");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SlowAdd() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/edit"),
            expectedRequestBody: "ac=subscribe&s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F&t=Science%21&a=user%2F-%2Flabel%2FTechnology", htmlResponse: "OK");

        await Inoreader.Subscriptions.Subscribe(new Uri("https://arstechnica.com/science/feed/"), "Science!", "Technology");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Rename() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/edit"),
            expectedRequestBody: "ac=edit&s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F&t=Science%21", htmlResponse: "OK");

        await Inoreader.Subscriptions.Rename(new Uri("https://arstechnica.com/science/feed/"), "Science!");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddToFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/edit"),
            expectedRequestBody: "ac=edit&s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F&a=user%2F-%2Flabel%2FTechnology", htmlResponse: "OK");

        await Inoreader.Subscriptions.AddToFolder(new Uri("https://arstechnica.com/science/feed/"), "Technology");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveFromFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/edit"),
            expectedRequestBody: "ac=edit&s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F&r=user%2F-%2Flabel%2FTechnology", htmlResponse: "OK");

        await Inoreader.Subscriptions.RemoveFromFolder(new Uri("https://arstechnica.com/science/feed/"), "Technology");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Unsubscribe() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(verb: HttpMethod.Post, url: new Uri("https://www.inoreader.com/reader/api/0/subscription/edit"),
            expectedRequestBody: "ac=unsubscribe&s=feed%2Fhttps%3A%2F%2Farstechnica.com%2Fscience%2Ffeed%2F", htmlResponse: "OK");

        await Inoreader.Subscriptions.Unsubscribe(new Uri("https://arstechnica.com/science/feed/"));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}