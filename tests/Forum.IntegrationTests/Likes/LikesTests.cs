using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public class LikesTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Fact]
    public async Task GivenOwnPost_WhenLiking_ThenConflict()
    {
        using var author = await Login("alex@example.test"); var id = await Post(author);
        Assert.Equal(HttpStatusCode.Conflict, (await author.PostAsync($"/api/v1/posts/{id}/likes", null)).StatusCode);
    }
    [Fact]
    public async Task GivenConcurrentLikes_WhenSameUser_ThenOnlyOnePersists()
    {
        using var author = await Login("alex@example.test"); var id = await Post(author);
        using var other = await Login("priya@example.test");
        var responses = await Task.WhenAll(other.PostAsync($"/api/v1/posts/{id}/likes", null), other.PostAsync($"/api/v1/posts/{id}/likes", null));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.NoContent);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        var post = await other.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal(1, post.GetProperty("likeCount").GetInt32());
    }
}
