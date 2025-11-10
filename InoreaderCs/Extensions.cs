using InoreaderCs.Marshal;
using System.Text.Json.Serialization;

namespace InoreaderCs;

internal static class Extensions {

    /// <summary>
    /// <para>Convert <see cref="DateTimeOffset"/> to microseconds since the Unix epoch.</para>
    /// <para>A microsecond is 1/1000th of a millisecond, or 1/1,000,000th of a second, or 1000 nanoseconds.</para>
    /// </summary>
    /// <param name="dateTimeOffset">Source time.</param>
    /// <returns>Number of microseconds between January 1, 1970 at midnight UTC to <paramref name="dateTimeOffset"/>.</returns>
    public static long ToUnixTimeMicroseconds(this DateTimeOffset dateTimeOffset) => (dateTimeOffset.UtcTicks - DateTimeOffsetReader.UnixEpochTicks) / 10;

    /// <summary>
    /// <para>If a <see cref="JsonConverter{T}"/> must be able to populate fields with both nullable and non-nullable value/struct types, you actually need two converter classes. This extension method lets you easily copy a nullable converter to a non-nullable variant that can also be registered. Example:</para>
    /// <para><c>
    /// JsonConverter&lt;DateTimeOffset?&gt; dateTimeOffsetConverter = new DateTimeOffsetReader();
    /// JsonSerializerOptions options = new() {
    ///     Converters = {
    ///         dateTimeOffsetConverter,
    ///         dateTimeOffsetConverter.ToNonNullable()
    ///     }
    /// }
    /// </c></para>
    /// <para>Now the following data class can be deserialized without crashing.</para>
    /// <para><c>
    /// public record MyData {
    ///     public DateTimeOffset Created { get; set; }
    ///     public DateTimeOffset? Updated { get; set; }
    /// }</c></para>
    /// </summary>
    /// <typeparam name="T">Non-nullable value/struct type</typeparam>
    /// <param name="nullableConverter"><see cref="JsonConverter{T}"/> that produces nullable <typeparamref name="T"/><c>?</c> values.</param>
    /// <returns></returns>
    public static JsonConverter<T> ToNonNullable<T>(this JsonConverter<T?> nullableConverter) where T: struct => new NonNullableStructReader<T>(nullableConverter);

}