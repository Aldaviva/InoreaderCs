using System.Text.Json;

namespace Tests.Marshal;

public class DateTimeOffsetReaderTest {

    private static readonly DateTimeOffsetReader DateTimeOffsetReader = new();

    private static readonly JsonSerializerOptions JsonOptions = new() {
        Converters = {
            DateTimeOffsetReader,
            new NonNullableStructReader<DateTimeOffset>(DateTimeOffsetReader)
        }
    };

    [Theory]
    [MemberData(nameof(AutoRangingData))]
    public void AutoRanging(long jsonNumber, DateTimeOffset expected, string description) {
        string         json   = jsonNumber.ToString();
        DateTimeOffset actual = JsonSerializer.Deserialize<DateTimeOffset>(json, JsonOptions);
        actual.Should().Be(expected, description);
    }

    public static TheoryData<long, DateTimeOffset, string> AutoRangingData => new() {
        { 1761688228, new DateTimeOffset(2025, 10, 28, 21, 50, 28, TimeSpan.Zero), "seconds" },
        { 1761688228815, new DateTimeOffset(2025, 10, 28, 21, 50, 28, 815, TimeSpan.Zero), "milliseconds" },
        { 1761688228815123, new DateTimeOffset(2025, 10, 28, 21, 50, 28, 815, 123, TimeSpan.Zero), "microseconds" }
    };

    [Fact]
    public void Errors() {
        var thrower = () => JsonSerializer.Deserialize<DateTimeOffset>("9999999999999999999999999999999999999999999999", JsonOptions);
        thrower.Should().Throw<JsonException>().WithInnerException<FormatException>();

        thrower = () => JsonSerializer.Deserialize<DateTimeOffset>("{}", JsonOptions);
        thrower.Should().Throw<JsonException>().WithMessage("number=null, TokenType=StartObject");
    }

    [Fact]
    public void DateTimeOffsetReaderDoesNotWrite() {
        MemoryStream         dest   = new();
        using Utf8JsonWriter writer = new(dest);
        new DateTimeOffsetReader().Write(writer, DateTimeOffset.Now, JsonSerializerOptions.Default);
        dest.Length.Should().Be(0);
    }

}