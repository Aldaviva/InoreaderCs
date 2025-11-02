using System.Text.Json.Serialization;

namespace InoreaderCs.Auth;

/// <summary>
/// <para>Cached user authentication tokens that are saved for future requests, even across different executions of the program.</para>
/// <para>When implementing <see cref="IAuthTokenPersister"/>, you can subclass this record to provide custom serialization attributes, such as <see cref="JsonConverterAttribute"/>, on these properties if necessary.</para>
/// </summary>
public record PersistedAuthTokens {

    /// <inheritdoc cref="Oauth2TokenResponse.AccessToken" />
    public virtual string? AccessToken { get; set; }

    /// <inheritdoc cref="Oauth2TokenResponse.RefreshToken" />
    public virtual string? RefreshToken { get; set; }

    /// <inheritdoc cref="Oauth2TokenResponse.Expiration" />
    public virtual DateTimeOffset? Expiration { get; set; }

    /// <summary>
    /// User access token from a user's email address and password. Does not expire.
    /// </summary>
    public virtual string? PasswordAuthToken { get; set; }

    /// <summary>
    /// Copy an OAuth2 token into this persisted object.
    /// </summary>
    /// <param name="oauthToken">An OAuth2 token response from the Inoreader API.</param>
    /// <returns>This instance, mutated.</returns>
    public virtual PersistedAuthTokens Load(Oauth2TokenResponse oauthToken) {
        AccessToken  = oauthToken.AccessToken;
        RefreshToken = oauthToken.RefreshToken;
        Expiration   = oauthToken.Expiration;
        return this;
    }

    /// <summary>
    /// Copy all properties from another instance into this object, useful for 
    /// </summary>
    /// <param name="source">Another instance of this class, perhaps loaded from disk</param>
    /// <returns>This instance, mutated</returns>
    public virtual PersistedAuthTokens LoadDefaults(PersistedAuthTokens source) {
        if (AccessToken is null && source.AccessToken is not null) {
            AccessToken  = source.AccessToken;
            RefreshToken = source.RefreshToken;
            Expiration   = source.Expiration;
        }
        PasswordAuthToken ??= source.PasswordAuthToken;
        return this;
    }

}