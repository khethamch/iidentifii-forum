using System.Threading.RateLimiting;
using Forum.Infrastructure;
using Forum.Infrastructure.Data;
using Forum.Infrastructure.Entities;
using Forum.Api.Exceptions;
using Forum.Api.Extensions;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var local = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing");

builder.Services.AddForumInfrastructure(builder.Configuration, local);
builder.Services.AddForumApplication();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 65536);

var app = builder.Build();

app.UseMiddleware<CustomExceptionMiddleware>();
if (!local)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapOpenApi();
app.MapControllers();

if (local && builder.Configuration.GetValue("SeedDemo", true))
{
    using var scope = app.Services.CreateScope();
    await DbInitializer.Run(scope.ServiceProvider.GetRequiredService<AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
}
app.Run();
public partial class Program { }
