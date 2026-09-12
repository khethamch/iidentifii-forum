using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public class AuthenticationTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Theory]
    [InlineData("/api/v1/posts")]
    [InlineData("/api/v1/posts/1/comments")]
    public async Task GivenAnonymous_WhenReading_ThenAllowed(string path)
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
    }
    [Fact]
    public async Task GivenAnonymous_WhenCreatingPost_ThenUnauthorized()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/posts", new { title = "Hi", body = "Test" })).StatusCode);
    }
    [Fact]
    public async Task GivenRegisteredUser_WhenLoginAndPost_ThenIdentityIsAssignedByServer()
    {
        var client = factory.CreateClient(); var email = $"{Guid.NewGuid()}@example.test";
        var registration = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "ForumDemo!2026", displayName = "New partner", role = "Moderator" });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var signed = await Login(email); var id = await Post(signed);
        var post = await signed.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal("New partner", post.GetProperty("author").GetString());
    }
    [Fact]
    public async Task GivenModeratorRoleInRegistration_WhenAccountCreated_ThenRegularUserOnly()
    {
        using var client = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.test";
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "ForumDemo!2026", displayName = "Role check", role = "Moderator" });
        response.EnsureSuccessStatusCode();
        using var signed = await Login(email);
        var identity = await signed.GetFromJsonAsync<JsonElement>("/api/v1/auth/me");
        Assert.False(identity.GetProperty("isModerator").GetBoolean());
        Assert.Equal(HttpStatusCode.Forbidden, (await signed.PostAsync("/api/v1/posts/1/tags", null)).StatusCode);
    }
    [Theory]
    [InlineData("likes")]
    [InlineData("tags")]
    [InlineData("comments")]
    public async Task GivenAnonymous_WhenWriting_ThenUnauthorized(string action)
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync($"/api/v1/posts/1/{action}", new { body = "hello" })).StatusCode);
    }
    [Fact]
    public async Task GivenInvalidCredentials_WhenLoggingIn_ThenUnauthorized()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "missing@example.test", password = "wrong" })).StatusCode);
    }

}
