using InoreaderCs.Marshal;

namespace InoreaderCs;

internal static class Extensions {

    /// <summary>
    /// <para>Convert <see cref="DateTimeOffset"/> to microseconds since the Unix epoch.</para>
    /// <para>A microsecond is 1/1000th of a millisecond, or 1/1,000,000th of a second, or 1000 nanoseconds.</para>
    /// </summary>
    /// <param name="dateTimeOffset">Source time.</param>
    /// <returns>Number of microseconds between January 1, 1970 at midnight UTC to <paramref name="dateTimeOffset"/>.</returns>
    public static long ToUnixTimeMicroseconds(this DateTimeOffset dateTimeOffset) => (dateTimeOffset.UtcTicks - DateTimeOffsetReader.UnixEpochTicks) / 10;

}