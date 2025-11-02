using FakeItEasy.Configuration;
using InoreaderCs.Auth;
using System.Linq.Expressions;
using Tests.Mocking;
using Unfucked;

namespace Tests.Auth;

public class Oauth2ClientTest: IDisposable {

    private readonly Oauth2Parameters     _oauth2Params = new(123, "abc");
    private readonly IAuthTokenPersister  _persister    = A.Fake<IAuthTokenPersister>();
    private readonly IUnfuckedHttpHandler _httpHandler  = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    private readonly HttpRequestMocker    _requestMocker;
    private readonly IUnfuckedHttpClient  _http;
    private readonly Oauth2Client         _auth;

    private readonly IAnyCallConfigurationWithReturnTypeSpecified<Task<ConsentResult>> _showConsentPageToUser;

    public Oauth2ClientTest() {
        _http          = UnfuckedHttpClient.Create(_httpHandler);
        _requestMocker = new HttpRequestMocker(_httpHandler);
        _auth = A.Fake<Oauth2Client>(options => {
            options.CallsBaseMethods();
            options.WithArgumentsForConstructor([_oauth2Params, _persister, _http, null]);
        });

        A.CallTo(_auth).Where(call => call.Method.Name == "get_AuthorizationReceiverCallbackUrl").WithReturnType<Uri>().Returns(new Uri("http://localhost/oauth2/callback"));

        _showConsentPageToUser = A.CallTo(_auth).Where(call => call.Method.Name == "ShowConsentPageToUser").WithReturnType<Task<ConsentResult>>();

        A.CallTo(() => _httpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).Throws((HttpRequestMessage message, CancellationToken ct) =>
            new InvalidOperationException($"Unmocked HTTP {message.Method} request to {message.RequestUri?.AbsoluteUri} with body {message.Content.ReadAsString()}"));
    }

    [Fact]
    public async Task Hit() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns(new PersistedAuthTokens {
            AccessToken = "def",
            Expiration  = DateTimeOffset.Now.AddDays(1)
        });

        IUserAuthToken actual = await _auth.FetchValidUserToken();

        actual.AuthenticationHeaderValue.Scheme.Should().Be("Bearer");
        actual.AuthenticationHeaderValue.Parameter.Should().Be("def");

        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Miss() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        _showConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult("ghi", consentUri.GetQuery()["state"], null, null));

        Expression<Func<Task<HttpResponseMessage>>> authorizationRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "code=ghi&redirect_uri=http%3A%2F%2Flocalhost%2Foauth2%2Fcallback&scope=&client_id=123&client_secret=abc&grant_type=authorization_code",
            """
            {
                "access_token": "jkl",
                "token_type": "Bearer",
                "expires_in": 86400,
                "refresh_token": "mno",
                "scope": "read"
            }
            """);

        IUserAuthToken actual = await _auth.FetchValidUserToken();

        actual.AuthenticationHeaderValue.Scheme.Should().Be("Bearer");
        actual.AuthenticationHeaderValue.Parameter.Should().Be("jkl");

        A.CallTo(authorizationRequest).MustHaveHappenedOnceExactly();
        _showConsentPageToUser.MustHaveHappenedOnceExactly();
        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>.That.Matches(match =>
            match.AccessToken == "jkl" &&
            match.RefreshToken == "mno" &&
            match.Expiration.HasValue &&
            match.Expiration.Value - DateTimeOffset.Now.AddHours(24) < TimeSpan.FromMinutes(1)))
        ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Refresh() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns(new PersistedAuthTokens {
            AccessToken  = "def",
            RefreshToken = "mno",
            Expiration   = DateTimeOffset.Now.AddMinutes(4.9)
        });

        Expression<Func<Task<HttpResponseMessage>>> authorizationRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "refresh_token=mno&client_id=123&client_secret=abc&grant_type=refresh_token",
            """
            {
                "access_token": "jkl",
                "token_type": "Bearer",
                "expires_in": 86400,
                "refresh_token": "pqr",
                "scope": "read"
            }
            """);

        IUserAuthToken actual = await _auth.FetchValidUserToken();

        actual.AuthenticationHeaderValue.Scheme.Should().Be("Bearer");
        actual.AuthenticationHeaderValue.Parameter.Should().Be("jkl");

        A.CallTo(authorizationRequest).MustHaveHappenedOnceExactly();
        _showConsentPageToUser.MustNotHaveHappened();
        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>.That.Matches(match =>
            match.AccessToken == "jkl" &&
            match.RefreshToken == "pqr" &&
            match.Expiration.HasValue &&
            match.Expiration.Value - DateTimeOffset.Now.AddHours(24) < TimeSpan.FromMinutes(1)))
        ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ConsentDenied() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        _showConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult(null, null, "access_denied", "User canceled"));

        Func<Task<IUserAuthToken>> thrower = () => _auth.FetchValidUserToken();
        thrower.Should().ThrowAsync<InoreaderException.Unauthorized>().WithMessage("Application was denied access to your Inoreader account");

        _showConsentPageToUser.MustHaveHappenedOnceExactly();
    }

    public void Dispose() {
        _http.Dispose();
        _auth.Dispose();
        GC.SuppressFinalize(this);
    }

}