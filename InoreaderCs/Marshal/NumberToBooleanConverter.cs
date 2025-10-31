using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

internal class NumberToBooleanConverter: JsonConverter<bool> {

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number && reader.GetByte() == 1;

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) { }

}