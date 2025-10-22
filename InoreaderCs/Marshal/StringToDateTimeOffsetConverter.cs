using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

public class StringToDateTimeOffsetConverter: JsonConverter<DateTimeOffset> {

    private const long UnixEpochTicks = 621355968000000000;

    /// <exception cref="JsonException"></exception>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        try {
            long? number = reader.TokenType switch {
                JsonTokenType.String => Convert.ToInt64(reader.GetString()),
                JsonTokenType.Number => reader.GetInt64(),
                _                    => null
            };
            return number is not (null or 0)
                ? new DateTimeOffset((number.Value >= 1000000000000000 ? number.Value * 10 : number.Value * 10000) + UnixEpochTicks, TimeSpan.Zero)
                : throw new JsonException();
        } catch (FormatException e) {
            throw new JsonException(null, e);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) { }

}