using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Forum.IntegrationTests;

public abstract class ForumTestBase
{
    protected readonly ForumFactory factory;
    protected ForumTestBase(ForumFactory factory) => this.factory = factory;
    protected async Task<HttpClient> Login(string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "ForumDemo!2026" });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("token").GetString());
        return client;
    }
    protected static async Task<int> Post(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/posts", new { title = "Testing a behaviour", body = "A useful integration question." });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }
}
