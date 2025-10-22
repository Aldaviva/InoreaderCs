using InoreaderCs.RateLimiting;

namespace InoreaderCs;

public class InoreaderException(string message, Exception cause): ApplicationException(message, cause) {

    public class Unauthorized(string message, Exception cause): InoreaderException(message, cause);

    public class RateLimited(RateLimitStatistics stats, Exception cause): InoreaderException("Rate limited", cause) {

        public RateLimitStatistics Statistics => stats;

    }

}