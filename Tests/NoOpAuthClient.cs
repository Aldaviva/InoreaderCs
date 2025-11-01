using InoreaderCs.Auth;
using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace Tests.Requests;

public class NoOpAuthClient: IAuthClient {

    public async Task<IUserAuthToken> FetchValidUserToken() => new NoOpAuthToken();

    public void Dispose() => GC.SuppressFinalize(this);

    private class NoOpAuthToken: IUserAuthToken {

        public AuthenticationHeaderValue AuthenticationHeaderValue => new("anonymous", null);
        public IDictionary<string, object>? RequestHeaders => ImmutableDictionary<string, object>.Empty;

    }

}