namespace InoreaderCs.Auth;

/// <summary>
/// <para>Saves and loads user authentication tokens to durable storage, such as files on disk or a database.</para>
/// <para>You must provided your own implementation of this interface by subclassing it. There are no built-in subclasses because the persistence logic is up to you.</para>
/// </summary>
public interface IAuthTokenPersister {

    /// <summary>
    /// Load user auth tokens from durable storage.
    /// </summary>
    /// <returns>Stored auth tokens, or <c>null</c> if none were found.</returns>
    Task<PersistedAuthTokens?> LoadAuthTokens();

    /// <summary>
    /// Save user auth tokens to durable storage.
    /// </summary>
    /// <param name="authTokens">User auth tokens to store.</param>
    Task SaveAuthTokens(PersistedAuthTokens authTokens);

}