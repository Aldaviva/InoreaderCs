namespace InoreaderCs.RateLimit;

/// <summary>
/// <para>Quota and usage of the Inoreader API rate limit returned in a response.</para>
/// <para>All usage resets every day at midnight in <c>Europe/Sofia</c>. This is affected by Daylight Saving Time, so when Bulgaria enters Eastern European Summer Time in on March 30, you will have to spread out your same number of limited requests over 25 hours, but when Bulgaria enters Eastern European Time on October 26, you will only have 23 hours to use up your same number of requests. For more details, see <see href="https://www.timeanddate.com/time/zone/bulgaria"/>.</para>
/// </summary>
/// <param name="Zone1Used">Number of read requests sent by this app.</param>
/// <param name="Zone2Used">Number of write requests sent by this app.</param>
/// <param name="Zone1Limit">Total quota of read requests this app can make in a 24 hour period (not the number remaining in the day).</param>
/// <param name="Zone2Limit">Total quota of write requests this app can make in a 24 hour period (not the number remaining in the day).</param>
/// <param name="TimeRemainingBeforeReset">The amount of time between the response and the usage being reset, which occurs at midnight in <c>Europe/Sofia</c> every day.</param>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/rate-limiting"/></remarks>
public record RateLimitStatistics(int Zone1Used, int Zone2Used, int Zone1Limit, int Zone2Limit, TimeSpan TimeRemainingBeforeReset) {

    private static readonly TimeSpan RateLimitPeriod = TimeSpan.FromHours(24);

    /// <summary>
    /// How long since the usage last reset.
    /// </summary>
    public TimeSpan TimePeriodElapsed => RateLimitPeriod - TimeRemainingBeforeReset;

    /// <summary>
    /// How far through the current usage period we are, in the range <c>[0.0, 1.0]</c>.
    /// </summary>
    public double TimePeriodPercentElapsed => (double) TimePeriodElapsed.Ticks / RateLimitPeriod.Ticks;

    /// <summary>
    /// <para>How on track we are to use up all of our quota if requests continue linearly at the same running rate as since the quota period started.</para>
    /// <para>This is not the percentage of the limit that has been used so far. This is a prediction of the future usage at the end of the quota period.</para>
    /// <para><c>1.0</c> means the quota will be completely exhausted right when usage resets.</para>
    /// <para>Percentages in the range <c>(0.0, 1.0)</c> represent predicted underusage of the quota.</para>
    /// <para>Percentages in the range <c>[1.0, ∞)</c> represent predicted overusage of the quota.</para>
    /// </summary>
    public double UtilizationRate => Math.Max(Zone1Used / (Zone1Limit * TimePeriodPercentElapsed), Zone2Used / (Zone2Limit * TimePeriodPercentElapsed));

}