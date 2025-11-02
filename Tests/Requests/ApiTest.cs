using Tests.Mocking;

namespace Tests.Requests;

public abstract class ApiTest: IDisposable {

    protected readonly IUnfuckedHttpHandler HttpHandler = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    protected readonly IInoreaderClient     Inoreader;
    protected readonly HttpRequestMocker    RequestMocker;

    protected ApiTest() {
        Inoreader = new InoreaderClient(new InoreaderOptions {
            AuthClient        = new NoOpAuthClient(),
            HttpClient        = UnfuckedHttpClient.Create(HttpHandler),
            DisposeHttpClient = true
        });
        RequestMocker = new HttpRequestMocker(HttpHandler);

        A.CallTo(() => HttpHandler.TestableSendAsync(An<HttpRequestMessage>._, A<CancellationToken>._)).Throws((HttpRequestMessage message, CancellationToken ct) =>
            new InvalidOperationException($"Unmocked HTTP {message.Method} request to {message.RequestUri?.AbsoluteUri} with body {message.Content.ReadAsString()}"));
    }

    public void Dispose() {
        Inoreader.Dispose();
        GC.SuppressFinalize(this);
    }

}