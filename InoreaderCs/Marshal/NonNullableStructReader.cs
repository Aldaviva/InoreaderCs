using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Marshal;

internal class NonNullableStructReader<T>(JsonConverter<T?> nullableConverter, bool thrownOnNullRead = true): JsonConverter<T> where T: struct {

    /// <exception cref="JsonException"></exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        nullableConverter.Read(ref reader, typeToConvert, options) ?? (thrownOnNullRead ? throw new JsonException() : default);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        nullableConverter.Write(writer, value, options);

}