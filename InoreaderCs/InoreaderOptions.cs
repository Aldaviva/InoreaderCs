using InoreaderCs.Auth;

namespace InoreaderCs;

/// <summary>
/// Parameters used in the construction of new <see cref="InoreaderClient"/> instances.
/// </summary>
public record InoreaderOptions {

    /// <summary>Provides authentication, using either OAuth2 (<see cref="Oauth2Client"/>) or a user's password (<see cref="PasswordAuthClient"/>). Required.</summary>
    public required IAuthClient AuthClient { get; set; }

    /// <summary>Optional HTTP client if you want to customize how requests and responses are handled, or <c>null</c> to use a default instance.</summary>
    public IHttpClient? HttpClient { get; set; }

    /// <summary>Whether <see cref="HttpClient"/> will be disposed along with this object. By default, it is only disposed when a custom <see cref="HttpClient"/> was provided and was not <c>null</c>.</summary>
    public bool? DisposeHttpClient { get; set; }

    /// <summary>
    /// How long to cache, after the last full update, the list of all labels in the user account and whether each one is a folder or tag. After this duration, this list will be automatically requested again from the API server the next time a method that needs it is called (such as <see cref="IInoreaderClient.INewsfeedMethods.ListArticlesDetailed"/>). The cache is also corrected when tags and folders are created using this client, but this correction does not extend the freshness timer or postpone a full update from the server.
    /// </summary>
    public TimeSpan LabelNameCacheDuration { get; set; } = TimeSpan.FromHours(1);

}