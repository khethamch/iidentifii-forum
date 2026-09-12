using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public class ModerationTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Fact]
    public async Task GivenModerator_WhenTagging_ThenAuditIsVisible()
    {
        using var author = await Login("alex@example.test"); var id = await Post(author);
        using var mod = await Login("moderator@example.test");
        Assert.Equal(HttpStatusCode.NoContent, (await mod.PostAsync($"/api/v1/posts/{id}/tags", null)).StatusCode);
        var post = await mod.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal("Morgan", post.GetProperty("tags")[0].GetProperty("moderator").GetString());
    }
    [Fact]
    public async Task GivenFlaggedPost_WhenFlaggingAgain_ThenConflict()
    {
        using var moderator = await Login("moderator@example.test");
        Assert.Equal(HttpStatusCode.Conflict, (await moderator.PostAsync("/api/v1/posts/3/tags", null)).StatusCode);
    }
    [Fact]
    public async Task GivenRegularUser_WhenFlagging_ThenForbidden()
    {
        using var client = await Login("alex@example.test");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/v1/posts/1/tags", null)).StatusCode);
    }
}
