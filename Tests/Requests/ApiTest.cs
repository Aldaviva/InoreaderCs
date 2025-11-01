using Tests.Mocking;

namespace Tests.Requests;

public abstract class ApiTest: IDisposable {

    private readonly   IUnfuckedHttpHandler _httpHandler = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    protected readonly IInoreaderClient     Inoreader;
    protected readonly HttpRequestMocker    RequestMocker;

    protected ApiTest() {
        Inoreader     = new InoreaderClient(new NoOpAuthClient(), UnfuckedHttpClient.Create(_httpHandler), true);
        RequestMocker = new HttpRequestMocker(_httpHandler);

        A.CallTo(() => _httpHandler.TestableSendAsync(A<HttpRequestMessage>._, A<CancellationToken>._)).Throws((HttpRequestMessage message, CancellationToken ct) =>
            new InvalidOperationException($"Unmocked HTTP {message.Method} request to {message.RequestUri?.AbsoluteUri} with body {message.Content.ReadAsString()}"));
    }

    public void Dispose() {
        Inoreader.Dispose();
        GC.SuppressFinalize(this);
    }

}