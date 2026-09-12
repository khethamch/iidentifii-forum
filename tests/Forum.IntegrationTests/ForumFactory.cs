using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Forum.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;

namespace Forum.IntegrationTests;

public sealed class ForumFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();
    private static string CreateConnectionString()
    {
        var connection = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("FORUM_TEST_SQLSERVER")
            ?? "Server=localhost,14339;Database=Forum;User Id=sa;Password=LocalForum!2026Demo;Encrypt=True;TrustServerCertificate=True");

        connection.InitialCatalog = $"ForumTests_{Guid.NewGuid():N}";
        return connection.ConnectionString;
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing")
        .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Forum"] = connectionString,
            ["SeedDemo"] = "true"
        }));
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options);
            db.Database.EnsureDeleted();
        }
    }
}
