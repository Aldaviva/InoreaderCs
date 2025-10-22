using System.Net.Http.Headers;

namespace InoreaderCs.Auth;

public interface IUserAuthToken {

    AuthenticationHeaderValue AuthenticationHeaderValue { get; }
    IDictionary<string, object>? RequestHeaders { get; }
    string Prefix { get; }

}

public class OAuthUserToken(Func<string> userTokenProvider): IUserAuthToken {

    public AuthenticationHeaderValue AuthenticationHeaderValue => new("Bearer", userTokenProvider());
    public IDictionary<string, object>? RequestHeaders => null;
    public string Prefix => "Bearer ";

}

public class AppUserToken(string userToken, long appId, string appKey): IUserAuthToken {

    public AuthenticationHeaderValue AuthenticationHeaderValue { get; } = new("GoogleLogin", "auth=" + userToken);

    public IDictionary<string, object>? RequestHeaders { get; } = new Dictionary<string, object> {
        ["AppId"]  = appId,
        ["AppKey"] = appKey
    };

    public string Prefix => "GoogleLogin auth=";

}