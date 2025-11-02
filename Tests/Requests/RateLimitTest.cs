using FluentAssertions.Events;
using FluentAssertions.Execution;
using FluentAssertions.Specialized;
using InoreaderCs.RateLimit;
using System.Net;
using System.Net.Mime;
using System.Text;
using Unfucked.HTTP.Exceptions;

namespace Tests.Requests;

public class RateLimitTest: ApiTest {

    [Fact]
    public async Task RateLimitedException() {
        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).ReturnsLazily(async (HttpRequestMessage req, CancellationToken ct) => new HttpResponseMessage {
            StatusCode     = HttpStatusCode.TooManyRequests,
            RequestMessage = req,
            Headers = {
                { "x-reader-zone1-usage", "5000" },
                { "x-reader-zone1-limit", "5000" },
                { "x-reader-zone2-usage", "1000" },
                { "x-reader-zone2-limit", "1000" },
                { "x-reader-limits-reset-after", "67790" }
            }
        });

        Task<ExceptionAssertions<InoreaderException.RateLimited>> expectation = Inoreader.Newsfeed
            .Awaiting(methods => methods.ListArticlesBrief())
            .Should().ThrowAsync<InoreaderException.RateLimited>()
            .WithMessage("Rate limited");
        await expectation.WithInnerException<InoreaderException.RateLimited, ClientErrorException>();

        RateLimitStatistics actual = (await expectation).And.Statistics;
        using (new AssertionScope()) {
            actual.Zone1Limit.Should().Be(5000);
            actual.Zone1Used.Should().Be(5000);
            actual.Zone2Limit.Should().Be(1000);
            actual.Zone2Used.Should().Be(1000);
            actual.TimeRemainingBeforeReset.Should().Be(TimeSpan.FromSeconds(67790));
            actual.TimePeriodElapsed.Should().Be(TimeSpan.FromSeconds(18610));
            actual.TimePeriodPercentElapsed.Should().BeApproximately(0.215, 0.001);
            actual.UtilizationRate.Should().BeApproximately(4.643, 0.001);
        }

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RateLimitEvent() {
        A.CallTo(() => HttpHandler.TestableSendAsync(
            An<HttpRequestMessage>.That.Matches(request => request.RequestUri == new Uri("https://www.inoreader.com/reader/api/0/user-info") && request.Method == HttpMethod.Get),
            A<CancellationToken>._)).ReturnsLazily(async (HttpRequestMessage request, CancellationToken ct) => new HttpResponseMessage {
            RequestMessage = request,
            StatusCode     = HttpStatusCode.OK,
            Headers = {
                { "x-reader-zone1-usage", "2500" },
                { "x-reader-zone1-limit", "5000" },
                { "x-reader-zone2-usage", "500" },
                { "x-reader-zone2-limit", "1000" },
                { "x-reader-limits-reset-after", "43200" }
            },
            Content = new StringContent(
                // language=json
                """{ "userId": "1006195123", "userName": "aldaviva", "userProfileId": "1006195123", "userEmail": "user@aldaviva.com", "isBloggerUser": false, "signupTimeSec": 1517740194, "isMultiLoginEnabled": false }""",
                Encoding.UTF8, MediaTypeNames.Application.Json)
        });

        using IMonitor<IInoreaderClient> eventListener = Inoreader.Monitor();

        _ = await Inoreader.Users.GetSelf();

        eventListener.Should().Raise(nameof(IInoreaderClient.RateLimitStatisticsReceived)).WithArgs<RateLimitStatistics>(stats =>
            stats.Zone1Used == 2500 &&
            stats.Zone1Limit == 5000 &&
            stats.Zone2Used == 500 &&
            stats.Zone2Limit == 1000 &&
            stats.TimePeriodElapsed == TimeSpan.FromHours(12) &&
            stats.TimeRemainingBeforeReset == TimeSpan.FromHours(12) &&
            Math.Abs(stats.TimePeriodPercentElapsed - 0.5) < 0.001 &&
            Math.Abs(stats.UtilizationRate - 1) < 0.001);
    }

}