using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Forum.Infrastructure.Configuration;
using Forum.Application.Dtos.Auth;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;

namespace Forum.Infrastructure.Services;

public sealed class JwtTokenGenerator(JwtSettings settings) : IJwtTokenGenerator
{
    public AuthResponseDto Generate(CurrentUserDto user, IEnumerable<string> roles)
    {
        var roleNames = roles.ToArray();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name)
        };

        claims.AddRange(roleNames.Select(role => new Claim(ClaimTypes.Role, role)));
        var expiry = DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes);
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            expires: expiry, signingCredentials: new SigningCredentials(settings.SigningKey, SecurityAlgorithms.HmacSha256));

        return new AuthResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expiry,
            new CurrentUserDto(user.Id, user.Name, roleNames.Contains(RoleNames.Moderator)));
    }
}
