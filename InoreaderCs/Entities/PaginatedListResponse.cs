using System.Text.Json;
using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// Response container envelope base class for paginated lists of objects.
/// </summary>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/stream-contents#:~:text=c%20%2D-,Continuation,-%2D%20a%20string%20used"/></remarks>
public abstract record PaginatedListResponse {

    /// <summary>
    /// <para>String that represents the handle to the pagination cursor, to be passed into a future request to get the subsequent page.</para>
    /// <para>Be aware that sometimes the API server ignores this value and returns the first page multiple times, so you may want to apply some uniqueness filtering on the client side.</para>
    /// <para>If this is <c>null</c>, it means there are no following pages and you have reached the end.</para>
    /// </summary>
    [JsonPropertyName("continuation")]
    public PaginationToken? PaginationToken { get; init; }

    /// <summary>
    /// Implicitly get the <see cref="PaginationToken"/> from this response, to make sending subsequent requests easier and more fluent.
    /// </summary>
    /// <param name="r"></param>
    public static implicit operator PaginationToken?(PaginatedListResponse r) => r.PaginationToken;

}

/// <summary>
/// <para>String that represents the handle to the pagination cursor, to be passed into a future request to get the subsequent page.</para>
/// <para>Be aware that sometimes the API server ignores this value and returns the first page multiple times, so you may want to apply some uniqueness filtering on the client side.</para>
/// </summary>
/// <param name="Continuation">pagination continuation token or cursor</param>
/// <remarks>Documentation: <see href="https://www.inoreader.com/developers/stream-contents#:~:text=c%20%2D-,Continuation,-%2D%20a%20string%20used"/></remarks>
[JsonConverter(typeof(Reader))]
[method: JsonConstructor]
public readonly record struct PaginationToken(string Continuation) {

    /// <inheritdoc />
    public override string ToString() => Continuation;

    internal class Reader: JsonConverter<PaginationToken> {

        /// <exception cref="JsonException"></exception>
        public override PaginationToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.String && reader.GetString() is { } continuation ? new PaginationToken(continuation) : throw new JsonException();

        public override void Write(Utf8JsonWriter writer, PaginationToken value, JsonSerializerOptions options) { }

    }

}