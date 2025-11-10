using InoreaderCs.Auth;
using System.Net;
using System.Net.Mime;
using System.Text;
using Unfucked.HTTP.Exceptions;

namespace Tests.Auth;

public class PasswordAuthClientTest: IDisposable {

    private readonly PasswordAuthParameters _password    = new("user@aldaviva.com", "abc123", 456, "789");
    private readonly IAuthTokenPersister    _persister   = A.Fake<IAuthTokenPersister>();
    private readonly IUnfuckedHttpHandler   _httpHandler = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    private readonly IHttpClient            _http;
    private readonly PasswordAuthClient     _auth;

    public PasswordAuthClientTest() {
        _http = UnfuckedHttpClient.Create(_httpHandler);
        _auth = new PasswordAuthClient(_password, _persister, _http, null);
    }

    [Fact]
    public async Task Hit() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns(new PersistedAuthTokens { PasswordAuthToken = "qwerty" });

        IUserAuthToken actual = await _auth.FetchValidUserToken();
        actual.AuthenticationHeaderValue.Scheme.Should().Be("GoogleLogin");
        actual.AuthenticationHeaderValue.Parameter.Should().Be("auth=qwerty");
        actual.RequestHeaders.Should().HaveCount(2).And.Contain(new Dictionary<string, object> {
            { "AppId", 456 },
            { "AppKey", "789" }
        });
    }

    [Fact]
    public async Task Miss() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        A.CallTo(() => _httpHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(req =>
            req.Method == HttpMethod.Post &&
            req.RequestUri == new Uri("https://www.inoreader.com/accounts/ClientLogin") &&
            req.Headers.UserAgent.ToString() == "Inoreader Android v7.9.6" &&
            req.Content.ReadAsString() == "Email=user%40aldaviva.com&Passwd=abc123&AppId=456&AppKey=789" &&
            req.Content!.Headers.ContentLanguage.Single() == "en_US"
        ), A<CancellationToken>._)).Returns(new HttpResponseMessage {
            Content = new StringContent(
                """
                SID=null
                LSID=null
                Auth=asdf
                Username=user
                Email=user@aldaviva.com
                Picture=https://www.inoreader.com/cdn/profile_picture/1006195123/5ytcddmU6y6x?s=128
                NewUser=0
                """,
                Encoding.UTF8, MediaTypeNames.Text.Html)
        });

        IUserAuthToken actual = await _auth.FetchValidUserToken();
        actual.AuthenticationHeaderValue.Scheme.Should().Be("GoogleLogin");
        actual.AuthenticationHeaderValue.Parameter.Should().Be("auth=asdf");
        actual.RequestHeaders.Should().HaveCount(2).And.Contain(new Dictionary<string, object> {
            { "AppId", 456 },
            { "AppKey", "789" }
        });

        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>.That.Matches(match => match.PasswordAuthToken == "asdf"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WrongCredentials() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        A.CallTo(() => _httpHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(req =>
            req.Method == HttpMethod.Post &&
            req.RequestUri == new Uri("https://www.inoreader.com/accounts/ClientLogin")
        ), A<CancellationToken>._)).Returns(new HttpResponseMessage(HttpStatusCode.Unauthorized));

        Func<Task<IUserAuthToken>> thrower = () => _auth.FetchValidUserToken();
        await thrower.Should().ThrowAsync<InoreaderException.Unauthorized>().WithMessage("Failed to create password auth token: 401")
            .WithInnerException<InoreaderException.Unauthorized, NotAuthorizedException>();
    }

    public void Dispose() {
        _auth.Dispose();
        _http.Dispose();
        GC.SuppressFinalize(this);
    }

}