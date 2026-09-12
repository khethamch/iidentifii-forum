using System.Net;
using System.Net.Http.Json;
namespace Forum.IntegrationTests;


public class RateLimitTests(ForumFactory factory) : IClassFixture<ForumFactory>
{
    [Fact]
    public async Task GivenThirtyAuthRequests_WhenAnotherArrives_ThenTooManyRequests()
    {
        using var client = factory.CreateClient();
        for (var i = 0; i < 30; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "missing@example.test", password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        var limited = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "missing@example.test", password = "wrong" });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
}
