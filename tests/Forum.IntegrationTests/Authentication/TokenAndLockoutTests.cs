using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Forum.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Forum.IntegrationTests;

public class TokenAndLockoutTests(ForumFactory factory) : ForumTestBase(factory), IClassFixture<ForumFactory>
{
    [Fact]
    public async Task GivenTamperedSignature_WhenReadingIdentity_ThenUnauthorized()
    {
        using var client = await Login("alex@example.test");
        var parts = client.DefaultRequestHeaders.Authorization!.Parameter!.Split('.');
        parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", string.Join('.', parts));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
    }

    [Fact]
    public async Task GivenExpiredSignedToken_WhenReadingIdentity_ThenUnauthorized()
    {
        using var client = factory.CreateClient();
        var settings = factory.Services.GetRequiredService<JwtSettings>();
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience,
            [new Claim(ClaimTypes.NameIdentifier, "alex")],
            notBefore: DateTime.UtcNow.AddMinutes(-10), expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(settings.SigningKey, SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
    }

    [Fact]
    public async Task GivenFiveFailedLogins_WhenCorrectPasswordIsSubmitted_ThenAccountRemainsLocked()
    {
        using var client = factory.CreateClient();
        var email = $"lockout-{Guid.NewGuid()}@example.test";
        (await client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "ForumDemo!2026", displayName = "Lockout test" })).EnsureSuccessStatusCode();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failed = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "WrongPassword!2026" });
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "ForumDemo!2026" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
