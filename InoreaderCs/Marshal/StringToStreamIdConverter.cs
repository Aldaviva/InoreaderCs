using InoreaderCs.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

public class StringToStreamIdConverter: JsonConverter<StreamId> {

    /// <exception cref="JsonException"></exception>
    public override StreamId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String && reader.GetString() is { } id ? StreamId.Parse(id) : throw new JsonException();

    public override void Write(Utf8JsonWriter writer, StreamId value, JsonSerializerOptions options) { }

}