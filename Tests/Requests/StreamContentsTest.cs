using System.Linq.Expressions;

namespace Tests.Requests;

public class StreamContentsTest: ApiTest {

    //language=json
    private const string ArticlesResponse =
        """
        { "direction": "ltr", "id": "user/1006195123/state/com.google/reading-list", "title": "Reading List", "description": "", "self": { "href": "https:\/\/www.inoreader.com\/reader\/api\/0\/stream\/contents\/user%2F1006195123%2Fstate%2Fcom.google%2Freading-list?n=1" }, "updated": 1761986767, "updatedUsec": "1761986767029232", "items": [{ "crawlTimeMsec": "1760450487167", "timestampUsec": "1760450487166567", "id": "tag:google.com,2005:reader\/item\/0000000ae763aaab", "categories": ["user\/1006195123\/state\/com.google\/reading-list", "user\/1006195123\/state\/com.google\/fresh", "user\/1006195123\/state\/com.google\/starred", "user\/1006195123\/label\/Technology", "user\/1006195123\/label\/To watch"], "title": "Netflix\u0026rsquo;s Splinter Cell: Deathwatch picks up where the games left off", "published": 1760450400, "updated": 0, "canonical": [{ "href": "https:\/\/www.theverge.com\/streaming\/799066\/splinter-cell-deathwatch-netflix-derek-kolstad-interview" }], "alternate": [{ "href": "https:\/\/www.theverge.com\/streaming\/799066\/splinter-cell-deathwatch-netflix-derek-kolstad-interview", "type": "text\/html" }], "summary": { "direction": "ltr", "content": "\u003Cimg alt=\u0022\u0022 src=\u0022https:\/\/platform.theverge.com\/wp-content\/uploads\/sites\/2\/2025\/10\/en_US_SCDW_2.00_00_32_19.Still001.jpg?quality=90\u0026amp;strip=all\u0026amp;crop=0,0,100,100\u0022\u003E \n\t \n\t\t \n \n\u003Cp\u003EIt\u0027s been more than a decade since there was a proper \u003Cem\u003ESplinter Cell\u003C\/em\u003E game. So when Derek Kolstad, a writer best-known for creating the \u003Cem\u003EJohn Wick\u003C\/em\u003E series, approached making the new animated Netflix adaptation, he treated it as if the story continued as the games were dormant. \u0022I wanted to do it almost like the timeline kept going since the last game,\u0022 Kolstad tells \u003Cem\u003EThe Verge\u003C\/em\u003E. At the outset of the show, called \u003Cem\u003EDeathwatch\u003C\/em\u003E, the iconic Sam Fisher finds himself in a new phase of life: \u0022Retired on a farm in the middle of nowhere, surprised that he\u0027s survived.\u0022 Of course, it\u0027s not long before things go wrong.\u003C\/p\u003E \n\u003Cp\u003E\u003Cem\u003EDeathwatch\u003C\/em\u003E sees this newly settled versio …\u003C\/p\u003E \n\u003Cp\u003E\u003Ca href=\u0022https:\/\/www.theverge.com\/streaming\/799066\/splinter-cell-deathwatch-netflix-derek-kolstad-interview\u0022\u003ERead the full story at The Verge.\u003C\/a\u003E\u003C\/p\u003E" }, "author": "Andrew Webster", "likingUsers": [], "comments": [], "commentsNum": -1, "annotations": [{ "id": 1127160852, "start": 0, "end": 0, "added_on": 1761986707, "text": "", "note": "Article note", "user_id": 1006195123, "user_name": "User", "user_profile_picture": "https:\/\/www.inoreader.com\/cdn\/profile_picture\/1006195123\/5ytcddmU6y6x?s=128" }], "origin": { "streamId": "feed\/https:\/\/www.theverge.com\/rss\/index.xml", "title": "The Verge", "htmlUrl": "http:\/\/www.theverge.com\/" }, "summaries": [] }], "continuation": "hXN46UBaS1ZT" }
        """;

    //language=json
    private const string StreamTypesResponseJson =
        """
        { "tags": [{ "id": "user\/1006195123\/state\/com.google\/starred", "sortid": "FFFFFFFF" }, { "id": "user\/1006195123\/state\/com.google\/broadcast", "sortid": "FFFFFFFE" }, { "id": "user\/1006195123\/state\/com.google\/blogger-following", "sortid": "FFFFFFFD" }, { "id": "user\/1006195123\/label\/Technology", "sortid": "BF5D7115", "unread_count": 36, "unseen_count": 1, "type": "folder", "pinned": 1, "article_count": 0, "article_count_today": 0 }, { "id": "user\/1006195123\/label\/To watch", "sortid": "BF70DD9A", "unread_count": 1, "unseen_count": 0, "type": "tag", "pinned": 1, "article_count": 1, "article_count_today": 0 }] }
        """;

    [Fact]
    public async Task ListArticlesInNewsfeed() {
        Expression<Func<Task<HttpResponseMessage>>> streamContentsRequest = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/contents/user%2F-%2Fstate%2Fcom.google%2Freading-list?n=1&includeAllDirectStreamIds=true&annotations=1"), null,
            ArticlesResponse);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        DetailedArticles actual = await Inoreader.Newsfeed.ListArticlesDetailed(1, showAnnotations: true);

        actual.Title.Should().Be("Reading List");
        actual.UpdateTime.Should().Be(new DateTimeOffset(2025, 11, 1, 1, 46, 07, 29, 232, TimeSpan.FromHours(-7)));
        actual.PaginationToken!.Value.Continuation.Should().Be("hXN46UBaS1ZT");

        Article article = actual.Articles.Should().ContainSingle().Subject;
        article.Author.Should().Be("Andrew Webster");
        article.CrawlTime.Should().Be(new DateTimeOffset(2025, 10, 14, 7, 1, 27, 166, 567, TimeSpan.FromHours(-7)));
        article.Description.Should().Be(
            "<img alt=\"\" src=\"https://platform.theverge.com/wp-content/uploads/sites/2/2025/10/en_US_SCDW_2.00_00_32_19.Still001.jpg?quality=90&amp;strip=all&amp;crop=0,0,100,100\"> \n\t \n\t\t \n \n<p>It's been more than a decade since there was a proper <em>Splinter Cell</em> game. So when Derek Kolstad, a writer best-known for creating the <em>John Wick</em> series, approached making the new animated Netflix adaptation, he treated it as if the story continued as the games were dormant. \"I wanted to do it almost like the timeline kept going since the last game,\" Kolstad tells <em>The Verge</em>. At the outset of the show, called <em>Deathwatch</em>, the iconic Sam Fisher finds himself in a new phase of life: \"Retired on a farm in the middle of nowhere, surprised that he's survived.\" Of course, it's not long before things go wrong.</p> \n<p><em>Deathwatch</em> sees this newly settled versio …</p> \n<p><a href=\"https://www.theverge.com/streaming/799066/splinter-cell-deathwatch-netflix-derek-kolstad-interview\">Read the full story at The Verge.</a></p>");
        article.FeedName.Should().Be("The Verge");
        article.FeedPageUrl.Should().Be(new Uri("http://www.theverge.com/"));
        article.FeedUrl.Should().Be(new Uri("https://www.theverge.com/rss/index.xml"));
        article.Folders.Should().ContainSingle().Which.Should().Be("Technology");
        article.IsRead.Should().BeFalse();
        article.IsStarred.Should().BeTrue();
        article.LongId.Should().Be("tag:google.com,2005:reader/item/0000000ae763aaab");
        article.PageUrl.Should().Be(new Uri("https://www.theverge.com/streaming/799066/splinter-cell-deathwatch-netflix-derek-kolstad-interview"));
        article.PublishTime.Should().Be(new DateTimeOffset(2025, 10, 14, 7, 0, 0, TimeSpan.FromHours(-7)));
        article.ShortId.Should().Be("46831741611");
        article.Tags.Should().ContainSingle().Which.Should().Be("To watch");
        article.Title.Should().Be("Netflix&rsquo;s Splinter Cell: Deathwatch picks up where the games left off");
        article.UpdateTime.Should().BeNull();

        Annotation annotation = article.Annotations.Should().ContainSingle().Subject;
        annotation.Id.Should().Be(1127160852);
        annotation.AddedOn.Should().Be(new DateTimeOffset(2025, 11, 1, 1, 45, 7, TimeSpan.FromHours(-7)));
        annotation.UserId.Should().Be(1006195123);
        annotation.UserFullName.Should().Be("User");
        annotation.UserProfilePicture.Should().Be(new Uri("https://www.inoreader.com/cdn/profile_picture/1006195123/5ytcddmU6y6x?s=128"));
        annotation.Note.Should().Be("Article note");
        annotation.Start.Should().Be(0);
        annotation.End.Should().Be(0);
        annotation.Text.Should().BeEmpty();

        A.CallTo(streamContentsRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task ListArticlesInFolder() {
        Expression<Func<Task<HttpResponseMessage>>> streamContentsRequest = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/contents/user%2F-%2Flabel%2FTechnology?n=1&includeAllDirectStreamIds=true&annotations=1"), null, ArticlesResponse);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        DetailedArticles actual = await Inoreader.Folders.ListArticlesDetailed("Technology", 1, showAnnotations: true);
        actual.Articles.Should().ContainSingle().Which.ShortId.Should().Be("46831741611");

        A.CallTo(streamContentsRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListArticlesInTag() {
        Expression<Func<Task<HttpResponseMessage>>> streamContentsRequest = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/contents/user%2F-%2Flabel%2FTo%20watch?n=1&includeAllDirectStreamIds=true&annotations=1"), null, ArticlesResponse);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        DetailedArticles actual = await Inoreader.Tags.ListArticlesDetailed("To watch", 1, showAnnotations: true);
        actual.Articles.Should().ContainSingle().Which.ShortId.Should().Be("46831741611");

        A.CallTo(streamContentsRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListArticlesInSubscription() {
        Expression<Func<Task<HttpResponseMessage>>> streamContentsRequest = RequestMocker.MockJsonHttpRequest(HttpMethod.Get,
            new Uri("https://www.inoreader.com/reader/api/0/stream/contents/feed%2Fhttps:%2F%2Fwww.theverge.com%2Frss%2Findex.xml?n=1&includeAllDirectStreamIds=true&annotations=1"), null,
            ArticlesResponse);
        Expression<Func<Task<HttpResponseMessage>>> streamTypesRequest =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamTypesResponseJson);

        DetailedArticles actual = await Inoreader.Subscriptions.ListArticlesDetailed(new Uri("https://www.theverge.com/rss/index.xml"), 1, showAnnotations: true);
        actual.Articles.Should().ContainSingle().Which.ShortId.Should().Be("46831741611");

        A.CallTo(streamContentsRequest).MustHaveHappenedOnceExactly();
        A.CallTo(streamTypesRequest).MustHaveHappenedOnceExactly();
    }

}