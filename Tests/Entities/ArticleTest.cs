namespace Tests.Entities;

public class ArticleTest {

    [Fact]
    public void Equality() {
        DateTimeOffset now = DateTimeOffset.Now;
        Article a1 = new() {
            LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
            Title     = string.Empty,
            Author    = string.Empty,
            CrawlTime = now
        };

        Article a2 = new() {
            LongId    = "tag:google.com,2005:reader/item/00000000148b9369",
            Title     = string.Empty,
            Author    = string.Empty,
            CrawlTime = now
        };

        Article b = new() {
            LongId    = "tag:google.com,2005:reader/item/00000000148b383e",
            Title     = string.Empty,
            Author    = string.Empty,
            CrawlTime = now.AddMinutes(-1)
        };

        a1.Equals(a2).Should().BeTrue();
        a1.Equals(b).Should().BeFalse();

        a1.GetHashCode().Should().Be(a2.GetHashCode());
        a1.GetHashCode().Should().NotBe(b.GetHashCode());
    }

}