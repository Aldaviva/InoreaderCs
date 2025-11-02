using Unfucked.HTTP.Exceptions;
using Unfucked.HTTP.Filters;

namespace InoreaderCs.RateLimit;

internal interface IRateLimitReader: ClientResponseFilter {

    event EventHandler<RateLimitStatistics>? StatisticsReceived;

}

/**
 * https://www.inoreader.com/developers/rate-limiting
 * Usage resets at midnight in Bulgaria (UTC+2 or UTC+3 DST), where Innologica is based
 */
internal class RateLimitReader: IRateLimitReader {

    private const int    OfficialAndroidAppZone1Limit = 20_000_000;
    public const  string RequestPropertyKey           = nameof(RateLimitStatistics);

    public event EventHandler<RateLimitStatistics>? StatisticsReceived;

    /// <inheritdoc />
    public async ValueTask<HttpResponseMessage> Filter(HttpResponseMessage response, FilterContext context, CancellationToken cancellationToken) {
        try {
            /*
             * If the Zone 1 limit is 20 million requests per day, then the rate limit is being counted against the official first-party Inoreader Android app, so ignore it because we're not in danger of running out.
             * Otherwise, if the Zone 1 limit is 5000, then the rate limit is being counted against my custom (second-party?) OAuth2 app, so count it so we can tell if we're about to exhaust our quota.
             */
            int zone1Limit = Convert.ToInt32(response.Headers.GetValues("X-Reader-Zone1-Limit").First());
            if (zone1Limit < OfficialAndroidAppZone1Limit) {
                int                 zone2Limit               = Convert.ToInt32(response.Headers.GetValues("X-Reader-Zone2-Limit").First());
                int                 zone1Used                = Convert.ToInt32(response.Headers.GetValues("X-Reader-Zone1-Usage").First());
                int                 zone2Used                = Convert.ToInt32(response.Headers.GetValues("X-Reader-Zone2-Usage").First());
                TimeSpan            timeRemainingBeforeReset = TimeSpan.FromSeconds(Convert.ToInt32(response.Headers.GetValues("X-Reader-Limits-Reset-After").First()));
                RateLimitStatistics statistics               = new(zone1Used, zone2Used, zone1Limit, zone2Limit, timeRemainingBeforeReset);

#pragma warning disable CS0618 // Type or member is obsolete - it's not obsolete in .NET Standard 2.0, which this library targets
                response.RequestMessage!.Properties[RequestPropertyKey] = statistics;
#pragma warning restore CS0618 // Type or member is obsolete

                StatisticsReceived?.Invoke(this, statistics);
            }
        } catch (InvalidOperationException) {
            // response does not contain rate-limiting headers
        } catch (FormatException e) {
            throw new ProcessingException(e, await HttpExceptionParams.FromResponse(response, cancellationToken).ConfigureAwait(false));
        }
        return response;
    }

#pragma warning disable CS0618 // Type or member is obsolete - it's not obsolete in .NET Standard 2.0, which this library targets

    public static RateLimitStatistics? Read(HttpResponseMessage response) => response.RequestMessage is { } request ? Read(request) : null;

    public static RateLimitStatistics? Read(HttpRequestMessage request) => request.Properties.TryGetValue(RequestPropertyKey, out object? stats) ? stats as RateLimitStatistics : null;

#pragma warning restore CS0618 // Type or member is obsolete

}