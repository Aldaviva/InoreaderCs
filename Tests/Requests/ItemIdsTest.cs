using System.Linq.Expressions;

namespace Tests.Requests;

public class ItemIdsTest: ApiTest {

    [Fact]
    public async Task ListBriefArticlesInNewsfeed() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=1&s=user/-/state/com.google/reading-list&includeAllDirectStreamIds=true"), null,
            """{ "items": [], "itemRefs": [{ "id": "47047259872", "directStreamIds": ["user\/1006195123\/label\/Technology"], "timestampUsec": "1761990925646284" }], "continuation": "2QD6qtUABtAE" }""");

        BriefArticles actual = await Inoreader.Newsfeed.ListArticlesBrief(1);

        BriefArticle article = actual.Articles.Should().ContainSingle().Subject;
        article.ShortId.Should().Be("47047259872");
        article.CrawlTime.Should().Be(new DateTimeOffset(2025, 11, 1, 2, 55, 25, 646, 284, TimeSpan.FromHours(-7)));
        article.FoldersAndTags.Should().ContainSingle().Which.Should().Be("Technology");
        actual.PaginationToken!.Value.Continuation.Should().Be("2QD6qtUABtAE");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task EmptyDeltaInNewsfeed() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=1&ot=1761990925646284&s=user/-/state/com.google/reading-list&includeAllDirectStreamIds=true"), null,
            """{ "items": [], "direction": "rtl" }""");

        BriefArticles actual = await Inoreader.Newsfeed.ListArticlesBrief(1, new DateTimeOffset(2025, 11, 1, 2, 55, 25, 646, 284, TimeSpan.FromHours(-7)));

        actual.Articles.Should().BeEmpty();

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListBriefArticlesInFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=1&s=user/-/label/Technology&includeAllDirectStreamIds=true"), null,
            """{ "items": [], "itemRefs": [{ "id": "47047259872", "directStreamIds": ["user\/1006195123\/label\/Technology"], "timestampUsec": "1761990925646284" }], "continuation": "2QD6qtUABtAE" }""");

        BriefArticles actual = await Inoreader.Folders.ListArticlesBrief("Technology", 1);

        BriefArticle article = actual.Articles.Should().ContainSingle().Subject;
        article.ShortId.Should().Be("47047259872");
        article.CrawlTime.Should().Be(new DateTimeOffset(2025, 11, 1, 2, 55, 25, 646, 284, TimeSpan.FromHours(-7)));
        article.FoldersAndTags.Should().ContainSingle().Which.Should().Be("Technology");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListBriefArticlesInTag() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/items/ids?n=1&s=user/-/label/My%20tag&includeAllDirectStreamIds=true"), null,
            """{ "items": [], "itemRefs": [{ "id": "47047259872", "directStreamIds": ["user\/1006195123\/label\/Technology", "user\/1006195123\/label\/My tag"], "timestampUsec": "1761990925646284" }], "continuation": "2QD6qtUABtAE" }""");

        BriefArticles actual = await Inoreader.Tags.ListArticlesBrief("My tag", 1);

        BriefArticle article = actual.Articles.Should().ContainSingle().Subject;
        article.ShortId.Should().Be("47047259872");
        article.CrawlTime.Should().Be(new DateTimeOffset(2025, 11, 1, 2, 55, 25, 646, 284, TimeSpan.FromHours(-7)));
        article.FoldersAndTags.Should().HaveCount(2).And.Contain(["Technology", "My tag"]);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}