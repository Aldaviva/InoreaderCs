using InoreaderCs.Marshal;

namespace InoreaderCs;

internal static class Extensions {

    public static long ToUnixTimeMicroseconds(this DateTimeOffset dateTimeOffset) => (dateTimeOffset.UtcTicks - StringToDateTimeOffsetReader.UnixEpochTicks) / 10;

}