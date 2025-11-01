using System.Linq.Expressions;

namespace Tests.Requests;

public class RenameTest: ApiTest {

    [Fact]
    public async Task RenameTag() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/rename-tag"),
            "s=user%2F-%2Flabel%2FOld+tag&dest=New+tag", "OK");

        await Inoreader.Tags.Rename("Old tag", "New tag");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RenameFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/rename-tag"),
            "s=user%2F-%2Flabel%2FOld+folder&dest=New+folder", "OK");

        await Inoreader.Folders.Rename("Old folder", "New folder");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task FolderNamesMustNotContainForwardSlashes() {
        Func<Task> thrower = async () => await Inoreader.Folders.Rename("Old folder", "New folder with / slash");
        await thrower.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

}