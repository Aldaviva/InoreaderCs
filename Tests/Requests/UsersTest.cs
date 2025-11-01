using System.Linq.Expressions;

namespace Tests.Requests;

public class UsersTest: ApiTest {

    [Fact]
    public async Task GetUser() {
        Expression<Func<Task<HttpResponseMessage>>> request = RequestMocker.MockJsonHttpRequest(verb: HttpMethod.Get, url: new Uri("https://www.inoreader.com/reader/api/0/user-info"),
            expectedRequestBody: null, jsonResponse:
            """{ "userId": "1006195123", "userName": "aldaviva", "userProfileId": "1006195123", "userEmail": "ben@aldaviva.com", "isBloggerUser": false, "signupTimeSec": 1517740194, "isMultiLoginEnabled": false }""");

        User actual = await Inoreader.Users.GetSelf();

        actual.Id.Should().Be(1006195123);
        actual.ProfileId.Should().Be(1006195123);
        actual.Username.Should().Be("aldaviva");
        actual.EmailAddress.Should().Be("ben@aldaviva.com");
        actual.SignupTime.Should().Be(new DateTimeOffset(2018, 2, 4, 2, 29, 54, TimeSpan.FromHours(-8)));

        A.CallTo(request).MustHaveHappenedOnceExactly();
    }

}