using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace Tests.Mocking;

public class HttpRequestMocker(IUnfuckedHttpHandler httpHandler) {

    public Expression<Func<Task<HttpResponseMessage>>> MockJsonHttpRequest(HttpMethod verb, Uri url, string? expectedRequestBody, [StringSyntax(StringSyntaxAttribute.Json)] string jsonResponse,
                                                                           HttpStatusCode status = HttpStatusCode.OK) =>
        MockRequest(verb, url, expectedRequestBody, jsonResponse, MediaTypeNames.Application.Json, status);

    public Expression<Func<Task<HttpResponseMessage>>> MockHtmlHttpRequest(HttpMethod verb, Uri url, string? expectedRequestBody, string htmlResponse, HttpStatusCode status = HttpStatusCode.OK) =>
        MockRequest(verb, url, expectedRequestBody, htmlResponse, MediaTypeNames.Text.Html, status);

    private Expression<Func<Task<HttpResponseMessage>>> MockRequest(HttpMethod verb, Uri url, string? expectedRequestBody, string responseBody, string responseContentType, HttpStatusCode status) {
        Expression<Func<Task<HttpResponseMessage>>> callSpecification = () => httpHandler.TestableSendAsync(A<HttpRequestMessage>.That.Matches(message =>
            message.Method == verb &&
            message.RequestUri == url &&
            (expectedRequestBody == null ? message.Content == null : expectedRequestBody == message.Content.ReadAsString())
        ), A<CancellationToken>._);

        A.CallTo(callSpecification).ReturnsLazily(async (HttpRequestMessage request, CancellationToken ct) => {
            if (request.Content != null) {
                request.Content = new UndisposableByteArrayContent(await request.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false));
            }
            return new HttpResponseMessage {
                StatusCode = status,
                Content    = new StringContent(responseBody, Encoding.UTF8, responseContentType)
            };
        });

        return callSpecification;
    }

}