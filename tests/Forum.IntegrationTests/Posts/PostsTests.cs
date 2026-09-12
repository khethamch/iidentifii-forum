using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public class PostsTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=100")]
    [InlineData("sort=invalid")]
    [InlineData("tag=invalid")]
    public async Task GivenInvalidQuery_WhenBrowsing_ThenBadRequest(string query) =>
        Assert.Equal(HttpStatusCode.BadRequest, (await factory.CreateClient().GetAsync($"/api/v1/posts?{query}")).StatusCode);
    [Fact]
    public async Task GivenEmptyPost_WhenSubmitting_ThenValidationError()
    {
        using var client = await Login("alex@example.test");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/posts", new { title = " ", body = "text" })).StatusCode);
    }
    [Fact]
    public async Task GivenMissingPost_WhenReading_ThenNotFound()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/posts/999999")).StatusCode);
    }
    [Fact]
    public async Task GivenFilters_WhenBrowsing_ThenReturnOnlyMatchingRows()
    {
        using var client = factory.CreateClient();
        var page = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?author=marcus&tag=flagged&sort=oldest&pageSize=1");
        Assert.Equal(1, page.GetProperty("total").GetInt32());
        Assert.Equal("marcus", page.GetProperty("items")[0].GetProperty("authorId").GetString());
    }
    [Fact]
    public async Task GivenFutureDateFilter_WhenBrowsing_ThenNoResults()
    {
        using var client = factory.CreateClient();
        var future = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?from=2099-01-01T00:00:00Z");
        Assert.Equal(0, future.GetProperty("total").GetInt32());
    }
    [Fact]
    public async Task GivenPages_WhenSortedByLikes_ThenStableAndDisjoint()
    {
        using var client = factory.CreateClient();
        var one = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?sort=likes&pageSize=2&page=1");
        var two = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?sort=likes&pageSize=2&page=2");
        var first = one.GetProperty("items").EnumerateArray().ToArray();
        var second = two.GetProperty("items").EnumerateArray().ToArray();
        Assert.Empty(first.Select(x => x.GetProperty("id").GetInt32()).Intersect(second.Select(x => x.GetProperty("id").GetInt32())));
        Assert.True(first.Last().GetProperty("likeCount").GetInt32() >= second.First().GetProperty("likeCount").GetInt32());
    }
}
