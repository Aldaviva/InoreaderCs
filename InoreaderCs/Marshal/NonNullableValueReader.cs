using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

internal class NonNullableValueReader<T>(JsonConverter<T?> nullableConverter): JsonConverter<T> where T: struct {

    /// <exception cref="JsonException"></exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        nullableConverter.Read(ref reader, typeToConvert, options) ?? throw new JsonException();

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) { }

}