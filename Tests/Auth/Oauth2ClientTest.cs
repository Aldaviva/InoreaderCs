using FakeItEasy.Configuration;
using InoreaderCs.Auth;
using System.Net;
using Tests.Mocking;
using Unfucked;
using Unfucked.HTTP.Exceptions;

namespace Tests.Auth;

public class Oauth2ClientTest: IDisposable {

    private readonly Oauth2Parameters     _oauth2Params = new(123, "abc");
    private readonly IAuthTokenPersister  _persister    = A.Fake<IAuthTokenPersister>();
    private readonly IUnfuckedHttpHandler _httpHandler  = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    private readonly HttpRequestMocker    _requestMocker;
    private readonly IHttpClient          _http;
    private readonly Oauth2Client         _auth;

    private IAnyCallConfigurationWithReturnTypeSpecified<Task<ConsentResult>> ShowConsentPageToUser =>
        A.CallTo(_auth).Where(call => call.Method.Name == "ShowConsentPageToUser").WithReturnType<Task<ConsentResult>>();

    public Oauth2ClientTest() {
        _http          = UnfuckedHttpClient.Create(_httpHandler);
        _requestMocker = new HttpRequestMocker(_httpHandler);
        _auth = A.Fake<Oauth2Client>(options => {
            options.CallsBaseMethods();
            options.WithArgumentsForConstructor([_oauth2Params, _persister, _http, null]);
        });

        A.CallTo(_auth).Where(call => call.Method.Name == "get_AuthorizationReceiverCallbackUrl").WithReturnType<Uri>().Returns(new Uri("http://localhost/oauth2/callback"));

        // _showConsentPageToUser ;

        A.CallTo(() => _httpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).Throws((HttpRequestMessage message, CancellationToken _) =>
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
        actual.RequestHeaders.Should().BeNull();

        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Miss() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult("ghi", consentUri.GetQuery()["state"], null, null));

        var authorizationRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
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
        ShowConsentPageToUser.MustHaveHappenedOnceExactly();
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

        var authorizationRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
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
        ShowConsentPageToUser.MustNotHaveHappened();
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

        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult(null, null, "access_denied", "User canceled"));

        var thrower = () => _auth.FetchValidUserToken();
        await thrower.Should().ThrowAsync<InoreaderException.Unauthorized>().WithMessage("Application was denied access to your Inoreader account");

        ShowConsentPageToUser.MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ReauthorizeWhenRefreshFails() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns(new PersistedAuthTokens {
            AccessToken  = "def",
            RefreshToken = "mno",
            Expiration   = DateTimeOffset.Now.AddMinutes(-5)
        });

        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult("ghi", consentUri.GetQuery()["state"], null, null));

        var failedRefreshRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "refresh_token=mno&client_id=123&client_secret=abc&grant_type=refresh_token", """{ "error_description": "your refresh token expired dawg" }""", HttpStatusCode.Unauthorized);

        var reauthRequest = _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "code=ghi&redirect_uri=http%3A%2F%2Flocalhost%2Foauth2%2Fcallback&scope=&client_id=123&client_secret=abc&grant_type=authorization_code", """
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

        A.CallTo(failedRefreshRequest).MustHaveHappenedOnceExactly();
        A.CallTo(reauthRequest).MustHaveHappenedOnceExactly();
        ShowConsentPageToUser.MustHaveHappenedOnceExactly();
        A.CallTo(() => _persister.SaveAuthTokens(A<PersistedAuthTokens>.That.Matches(match =>
            match.AccessToken == "jkl" &&
            match.RefreshToken == "pqr" &&
            match.Expiration.HasValue &&
            match.Expiration.Value - DateTimeOffset.Now.AddHours(24) < TimeSpan.FromMinutes(1)))
        ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Errors() {
        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult("ghi", consentUri.GetQuery()["state"], null, null));

        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        A.CallTo(() => _httpHandler.TestableSendAsync(A<HttpRequestMessage>._, A<CancellationToken>._))
            .ThrowsAsync((HttpRequestMessage request, CancellationToken _) => new ProcessingException(new IOException("test exception"), HttpExceptionParams.FromRequest(request))).Twice();

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<ProcessingException>();

        // _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
        //     "code=ghi&redirect_uri=http%3A%2F%2Flocalhost%2Foauth2%2Fcallback&scope=&client_id=123&client_secret=abc&grant_type=authorization_code",
        //     """
        //     {
        //         "access_token": "jkl",
        //         "token_type": "Bearer",
        //         "expires_in": 86400,
        //         "refresh_token": "mno",
        //         "scope": "read"
        //     }
        //     """, );

        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult(null, null, "test_error", "Test Error"));

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<InoreaderException.Unauthorized>().WithMessage("test_error: Test Error");

        A.CallTo(() => _persister.LoadAuthTokens()).Returns(new PersistedAuthTokens {
            AccessToken  = "def",
            RefreshToken = "mno",
            Expiration   = DateTimeOffset.Now
        });

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<ProcessingException>();

        _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "refresh_token=mno&client_id=123&client_secret=abc&grant_type=refresh_token",
            (string) "malformed json", HttpStatusCode.ServiceUnavailable);

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<ProcessingException>();

        _requestMocker.MockJsonHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/oauth2/token"),
            "refresh_token=mno&client_id=123&client_secret=abc&grant_type=refresh_token",
            """{ "error_description": 123 }""", HttpStatusCode.ServiceUnavailable);

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<ProcessingException>();
    }

    [Fact]
    public async Task WrongAntiforgeryToken() {
        A.CallTo(() => _persister.LoadAuthTokens()).Returns<PersistedAuthTokens?>(null);

        ShowConsentPageToUser.ReturnsLazily((Uri consentUri, Uri codeReceiverCallbackUri, Task authorizationSuccess) => new ConsentResult("ghi", "all ur base are belong to us", null, null));

        await _auth.Invoking(a => a.FetchValidUserToken()).Should().ThrowAsync<InoreaderException.Unauthorized>();
    }

    public void Dispose() {
        _http.Dispose();
        _auth.Dispose();
        GC.SuppressFinalize(this);
    }

}