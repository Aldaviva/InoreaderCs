using InoreaderCs.RateLimiting;

namespace InoreaderCs;

/// <summary>
/// Base class for exceptions thrown by the InoreaderCs library.
/// </summary>
/// <param name="message">Error description.</param>
/// <param name="cause">Cause of the error.</param>
public class InoreaderException(string message, Exception? cause): ApplicationException(message, cause) {

    /// <summary>
    /// Not authorized error.
    /// </summary>
    /// <param name="message">Error description.</param>
    /// <param name="cause">Cause of the error.</param>
    public class Unauthorized(string message, Exception? cause): InoreaderException(message, cause);

    /// <summary>Too many requests.</summary>
    /// <param name="stats">Rate limit quota and usage from the offending request.</param>
    /// <param name="cause">Cause of the error.</param>
    public class RateLimited(RateLimitStatistics stats, Exception cause): InoreaderException("Rate limited", cause) {

        /// <summary>
        /// Rate limit quota and usage from the offending request.
        /// </summary>
        public RateLimitStatistics Statistics => stats;

    }

}