using System.Security.Cryptography;
using System.Text;
using Forum.Infrastructure.Configuration;
using Forum.Infrastructure.Data;
using Forum.Infrastructure.Entities;
using Forum.Infrastructure.Services;
using Forum.Application.Interfaces.Repositories;
using Forum.Infrastructure.Repositories;
using Forum.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forum.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddForumInfrastructure(this IServiceCollection services, IConfiguration configuration, bool local)
    {
        var secret = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(secret))
        {
            if (!local) throw new InvalidOperationException("Set Jwt__Key to a random secret of at least 32 characters.");

            secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        if (secret.Length < 32) throw new InvalidOperationException("Jwt__Key must be at least 32 characters.");
        var jwt = new JwtSettings(configuration["Jwt:Issuer"] ?? "Forum",
            configuration["Jwt:Audience"] ?? "ForumClients",
            configuration.GetValue("Jwt:ExpiryMinutes", 30),
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)));
        if (jwt.ExpiryMinutes <= 0) throw new InvalidOperationException("Jwt:ExpiryMinutes must be positive.");
        services.AddSingleton(jwt);

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("Forum")
            ?? throw new InvalidOperationException("Configure ConnectionStrings__Forum for SQL Server.")));
        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwt.SigningKey,
                ClockSkew = TimeSpan.FromSeconds(10)
            };
        });
        services.AddAuthorization();

        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        return services;
    }
}
