using System.Net.Http.Headers;
using Unfucked;
using Unfucked.HTTP.Filters;

namespace InoreaderCs.Auth;

internal class InoreaderAuthenticationFilter(Func<IUserAuthToken> userAuthTokenProvider): ClientRequestFilter {

    public Task<HttpRequestMessage> Filter(HttpRequestMessage request, FilterContext context, CancellationToken cancellationToken) {
        HttpRequestHeaders headers = request.Headers;
        if (request.RequestUri!.BelongsToDomain(InoreaderClient.ApiBase) && headers.Authorization == null) {
            IUserAuthToken userAuthToken = userAuthTokenProvider();
            headers.Authorization = userAuthToken.AuthenticationHeaderValue;
            foreach (KeyValuePair<string, object> header in userAuthToken.RequestHeaders.AsEnumerable() ?? []) {
                headers.Add(header.Key, header.Value.ToString());
            }
        }
        return Task.FromResult(request);
    }

}