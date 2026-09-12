using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public class CommentsTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Fact]
    public async Task GivenComments_WhenPaging_ThenReturnBoundedResults()
    {
        using var client = await Login("alex@example.test"); var id = await Post(client);
        for (var i = 0; i < 3; i++) Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync($"/api/v1/posts/{id}/comments", new { body = $"Comment {i}" })).StatusCode);
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}/comments?page=2&pageSize=2");
        Assert.Equal(3, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("items").GetArrayLength());
    }
}
