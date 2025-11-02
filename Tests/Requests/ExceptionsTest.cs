using System.Linq.Expressions;
using System.Net;
using Unfucked.HTTP.Exceptions;

namespace Tests.Requests;

public class ExceptionsTest: ApiTest {

    [Fact]
    public async Task Unauthorized() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockHtmlHttpRequest(HttpMethod.Get,
                new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=20&s=user/-/state/com.google/reading-list&includeAllDirectStreamIds=true"), null, string.Empty,
                HttpStatusCode.Unauthorized);

        await Inoreader.Newsfeed.Invoking(methods => methods.ListArticlesBrief())
            .Should().ThrowAsync<InoreaderException.Unauthorized>()
            .WithMessage("Inoreader auth failure")
            .WithInnerException<InoreaderException.Unauthorized, NotAuthorizedException>();

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}