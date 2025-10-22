namespace InoreaderCs.RateLimiting;

public record RateLimitStatistics(int Zone1Used, int Zone2Used, int Zone1Limit, int Zone2Limit, TimeSpan TimeRemainingBeforeReset) {

    private static readonly TimeSpan RateLimitPeriod = TimeSpan.FromHours(24);

    public TimeSpan TimePeriodElapsed => RateLimitPeriod - TimeRemainingBeforeReset;
    public double TimePeriodPercentElapsed => (double) TimePeriodElapsed.Ticks / RateLimitPeriod.Ticks;
    public double UtilizationRate => Math.Max(Zone1Used / (Zone1Limit * TimePeriodPercentElapsed), Zone2Used / (Zone2Limit * TimePeriodPercentElapsed));

}