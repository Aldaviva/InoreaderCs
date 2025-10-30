using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

internal class DateTimeOffsetReader: JsonConverter<DateTimeOffset?> {

    internal const long UnixEpochTicks = 621355968000000000;

    /// <exception cref="JsonException"></exception>
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        try {
            JsonTokenType tokenType = reader.TokenType;
            long? number = tokenType switch {
                JsonTokenType.String => Convert.ToInt64(reader.GetString()),
                JsonTokenType.Number => reader.GetInt64(),
                _                    => null
            };
            return number switch {
                0     => null,
                { } n => new DateTimeOffset(AutoRangeToTicksSinceUnixEpoch(n) + UnixEpochTicks, TimeSpan.Zero),
                _     => throw new JsonException($"{nameof(number)}={number}, {nameof(reader.TokenType)}={tokenType}")
            };
        } catch (FormatException e) {
            throw new JsonException(null, e);
        }
    }

    private static long AutoRangeToTicksSinceUnixEpoch(long mysteryTime) => mysteryTime * mysteryTime switch {
        >= 100000000000000 => 10,      // microseconds
        >= 100000000000    => 10000,   // milliseconds
        _                  => 10000000 // seconds
    };

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options) { }

}