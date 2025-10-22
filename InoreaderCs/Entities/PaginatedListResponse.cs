using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public abstract record PaginatedListResponse {

    [JsonPropertyName("continuation")]
    public PaginationToken? PaginationToken { get; init; }

    public static implicit operator PaginationToken?(PaginatedListResponse r) => r.PaginationToken;

}

[JsonConverter(typeof(Reader))]
[method: JsonConstructor]
public readonly record struct PaginationToken(string Continuation) {

    public override string ToString() => Continuation;

    internal class Reader: JsonConverter<PaginationToken> {

        /// <exception cref="JsonException"></exception>
        public override PaginationToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.String && reader.GetString() is { } continuation ? new PaginationToken(continuation) : throw new JsonException();

        public override void Write(Utf8JsonWriter writer, PaginationToken value, JsonSerializerOptions options) { }

    }

}