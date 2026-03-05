using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Marshal;

public class ReaderTests {

    [Fact]
    public void NumberToBooleanReaderDoesNotWrite() {
        MemoryStream         dest   = new();
        using Utf8JsonWriter writer = new(dest);
        new NumberToBooleanReader().Write(writer, true, JsonSerializerOptions.Default);
        dest.Length.Should().Be(0);
    }

    [Fact]
    public void StringToStreamIdReaderDoesNotWrite() {
        MemoryStream         dest   = new();
        using Utf8JsonWriter writer = new(dest);
        new StringToStreamIdReader().Write(writer, StreamId.Read, JsonSerializerOptions.Default);
        dest.Length.Should().Be(0);
    }

    [Fact]
    public void PaginationTokenReaderDoesNotWrite() {
        MemoryStream         dest   = new();
        using Utf8JsonWriter writer = new(dest);
        new PaginationToken.Reader().Write(writer, new PaginationToken("abcdef"), JsonSerializerOptions.Default);
        dest.Length.Should().Be(0);
    }

    [Fact]
    public void NonNullableWrite() {
        MemoryStream         dest                 = new();
        using Utf8JsonWriter writer               = new(dest);
        var                  nullableConverter    = A.Fake<JsonConverter<DateTimeOffset?>>();
        var                  nonNullableConverter = nullableConverter.ToNonNullable();
        nonNullableConverter.Write(writer, DateTimeOffset.Now, JsonSerializerOptions.Default);

        A.CallTo(() => nullableConverter.Write(A<Utf8JsonWriter>._, A<DateTimeOffset?>._, A<JsonSerializerOptions>._)).MustHaveHappened();
    }

}