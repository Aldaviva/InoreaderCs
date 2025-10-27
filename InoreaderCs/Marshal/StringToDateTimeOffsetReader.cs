using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

internal class StringToDateTimeOffsetReader: JsonConverter<DateTimeOffset?> {

    private const long UnixEpochTicks = 621355968000000000;

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
                0        => null,
                not null => new DateTimeOffset((number.Value >= 1000000000000000 ? number.Value * 10 : number.Value * 10000) + UnixEpochTicks, TimeSpan.Zero),
                _        => throw new JsonException($"{nameof(number)}={number}, {nameof(reader.TokenType)}={tokenType}")
            };
        } catch (FormatException e) {
            throw new JsonException(null, e);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options) { }

}