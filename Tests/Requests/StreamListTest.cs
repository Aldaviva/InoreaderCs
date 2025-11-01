using System.Linq.Expressions;

namespace Tests.Requests;

public class StreamListTest: ApiTest {

    //language=json
    private const string StreamListResponse =
        """
        { "tags": [{ "id": "user\/1006195123\/state\/com.google\/starred", "sortid": "FFFFFFFF" }, { "id": "user\/1006195123\/state\/com.google\/broadcast", "sortid": "FFFFFFFE" }, { "id": "user\/1006195123\/state\/com.google\/blogger-following", "sortid": "FFFFFFFD" }, { "id": "user\/1006195123\/label\/Comics", "sortid": "BF3834D6", "unread_count": 3, "unseen_count": 1, "type": "folder", "pinned": 1, "article_count": 0, "article_count_today": 0 }, { "id": "user\/1006195123\/label\/Telco news", "sortid": "BF844B22", "unread_count": 1, "unseen_count": 0, "type": "tag", "pinned": 1, "article_count": 9, "article_count_today": 2 }] }
        """;

    [Fact]
    public async Task ListFolders() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamListResponse);

        IEnumerable<FolderState> actual = await Inoreader.Folders.List();

        FolderState folder = actual.Should().ContainSingle().Subject;
        folder.Name.Should().Be("Comics");
        folder.UnreadCount.Should().Be(3);
        folder.UnseenCount.Should().Be(1);

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ListTags() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockJsonHttpRequest(HttpMethod.Get, new Uri("https://www.inoreader.com/reader/api/0/tag/list?types=1&counts=1"), null, StreamListResponse);

        IEnumerable<TagState> actual = await Inoreader.Tags.List();

        TagState tag = actual.Should().ContainSingle().Subject;
        tag.Name.Should().Be("Telco news");
        tag.UnreadCount.Should().Be(1);
        tag.UnseenCount.Should().Be(0);
        tag.ArticleCount.Should().Be(9);
        tag.ArticleCountToday.Should().Be(2);
        tag.IsPinned.Should().BeTrue();

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}