using System.Linq.Expressions;

namespace Tests.Requests;

public class DeleteTest: ApiTest {

    [Fact]
    public async Task DeleteTag() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/disable-tag"), "s=user%2F-%2Flabel%2FTag+to+delete", "OK");

        await Inoreader.Tags.Delete("Tag to delete");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteNonExistentTag() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/disable-tag"),
            "s=user%2F-%2Flabel%2FTag+to+delete", "Error=Tag not found!");

        await Inoreader.Tags.Delete("Tag to delete");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteFolder() {
        Expression<Func<Task<HttpResponseMessage>>> request =
            RequestMocker.MockHtmlHttpRequest(HttpMethod.Post, new Uri("https://www.inoreader.com/reader/api/0/disable-tag"), "s=user%2F-%2Flabel%2FFolder+to+delete", "OK");

        await Inoreader.Folders.Delete("Folder to delete");

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}