using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
namespace Forum.IntegrationTests;

public class ControllerContractTests(ForumFactory factory) : IClassFixture<ForumFactory>
{
    [Theory]
    [InlineData("page=abc")]
    [InlineData("from=not-a-date")]
    public async Task GivenInvalidQueryType_WhenBinding_ThenValidationProblem(string query)
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/posts?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.True(problem.GetProperty("errors").EnumerateObject().Any());
    }

    [Fact]
    public async Task GivenMissingRegistrationFields_WhenSubmitting_ThenValidationProblem()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { email = "missing-fields@example.test" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").EnumerateObject().Any());
    }

    [Fact]
    public async Task GivenMalformedJson_WhenLoggingIn_ThenBadRequest()
    {
        using var client = factory.CreateClient();
        using var body = new StringContent("{invalid", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/auth/login", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenCreatedPost_WhenFollowingLocation_ThenRetrieveSamePost()
    {
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "alex@example.test", password = "ForumDemo!2026" });
        login.EnsureSuccessStatusCode();
        var session = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.GetProperty("token").GetString());

        var response = await client.PostAsJsonAsync("/api/v1/posts", new { title = "Controller location contract", body = "Can clients follow the created resource location?" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var post = await client.GetFromJsonAsync<JsonElement>(response.Headers.Location);
        Assert.Equal(created.GetProperty("id").GetInt32(), post.GetProperty("id").GetInt32());
    }
}
