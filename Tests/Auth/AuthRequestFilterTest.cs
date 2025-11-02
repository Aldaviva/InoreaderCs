using InoreaderCs.Auth;
using Unfucked.HTTP.Filters;

namespace Tests.Auth;

public class AuthRequestFilterTest {

    private readonly IAuthClient       _authClient = A.Fake<IAuthClient>();
    private readonly AuthRequestFilter _filter;

    public AuthRequestFilterTest() {
        _filter = new AuthRequestFilter(_authClient);
    }

    [Fact]
    public async Task ModifiesRequest() {
        A.CallTo(() => _authClient.FetchValidUserToken()).Returns(new UserPasswordToken("abc", 123, "def"));

        using HttpRequestMessage req = new(HttpMethod.Get, "https://www.inoreader.com/reader/api/0/user-info");

        using HttpRequestMessage filteredRequest = await _filter.Filter(req, new FilterContext(), CancellationToken.None);

        filteredRequest.Should().BeSameAs(req);
        filteredRequest.Headers.Authorization!.Scheme.Should().Be("GoogleLogin");
        filteredRequest.Headers.Authorization.Parameter.Should().Be("auth=abc");
        filteredRequest.Headers.GetValues("AppId").Should().ContainSingle().And.Contain("123");
        filteredRequest.Headers.GetValues("AppKey").Should().ContainSingle().And.Contain("def");
    }

    [Theory]
    [InlineData("https://www.inoreader.com.evilsite.com/reader/api/0/user-info")]
    [InlineData("https://evilsite.com/https://www.inoreader.com/reader/api/0/user-info")]
    public async Task SiteLock(string url) {
        A.CallTo(() => _authClient.FetchValidUserToken()).Returns(new UserPasswordToken("abc", 123, "def"));

        using HttpRequestMessage req = new(HttpMethod.Get, url);

        using HttpRequestMessage filteredRequest = await _filter.Filter(req, new FilterContext(), CancellationToken.None);

        filteredRequest.Headers.Authorization.Should().BeNull("wrong domain");
        filteredRequest.Headers.Should().NotContainKeys("AppId", "AppKey");
    }

}