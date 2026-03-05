namespace Tests.Entities;

public class StreamIdTest {

    [Fact]
    public void StreamIdEquality() {
        StreamId a1 = StreamId.Read;
        StreamId a2 = StreamId.Read;
        StreamId b  = StreamId.Starred;

        (a1 == a2).Should().BeTrue();
        (a1 != a2).Should().BeFalse();
        (a1 == b).Should().BeFalse();
        (a1 != b).Should().BeTrue();
    }

    [Fact]
    public void Errors() {
        var thrower = () => StreamId.Parse("hargle/blargle");
        thrower.Should().Throw<ArgumentOutOfRangeException>();
    }

}