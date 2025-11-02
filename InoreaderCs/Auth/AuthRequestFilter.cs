using System.Net.Http.Headers;
using Unfucked.HTTP.Exceptions;
using Unfucked.HTTP.Filters;

namespace InoreaderCs.Auth;

internal class AuthRequestFilter(IAuthClient authClient): ClientRequestFilter {

    /// <exception cref="ProcessingException"></exception>
    /// <exception cref="InoreaderException.Unauthorized"></exception>
    public async ValueTask<HttpRequestMessage> Filter(HttpRequestMessage request, FilterContext context, CancellationToken cancellationToken) {
        HttpRequestHeaders headers = request.Headers;
        if (request.RequestUri!.BelongsToDomain(InoreaderClient.ApiRoot) && headers.Authorization == null) {
            try {
                IUserAuthToken userAuthToken = await authClient.FetchValidUserToken().ConfigureAwait(false);

                headers.Authorization = userAuthToken.AuthenticationHeaderValue;

                foreach (KeyValuePair<string, object> header in userAuthToken.RequestHeaders?.AsEnumerable() ?? []) {
                    headers.Add(header.Key, header.Value.ToString());
                }
            } catch (FormatException e) {
                throw new ProcessingException(e, HttpExceptionParams.FromRequest(request));
            }
        }
        return request;
    }

}