using Microsoft.IdentityModel.Tokens;

namespace Forum.Infrastructure.Configuration;

public sealed record JwtSettings(string Issuer, string Audience, int ExpiryMinutes, SymmetricSecurityKey SigningKey);
