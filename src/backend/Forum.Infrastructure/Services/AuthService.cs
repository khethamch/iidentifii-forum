using System.Net.Mail;
using Forum.Domain.Rules;
using Forum.Application.Dtos.Auth;
using Forum.Domain.Entities;
using Forum.Infrastructure.Entities;
using Forum.Domain.Exceptions;
using Forum.Application.Exceptions;
using Forum.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Forum.Infrastructure.Services;

public sealed class AuthService(UserManager<User> users, IJwtTokenGenerator tokens) : IAuthService
{
    public async Task<RegisterResponseDto> Register(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 254
            || !MailAddress.TryCreate(request.Email, out var address) || address.Address != request.Email.Trim())
            throw new ForumException("Enter a valid email address.");

        var user = new User
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = ForumRules.RequiredText(request.DisplayName, 80)
        };

        var result = await users.CreateAsync(user, request.Password ?? "");
        if (!result.Succeeded)
            throw new RequestValidationException(result.Errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

        var assignment = await users.AddToRoleAsync(user, RoleNames.For(UserRole.Regular));
        if (!assignment.Succeeded)
            throw new InvalidOperationException("The regular user role could not be assigned.");
        return new RegisterResponseDto(user.Id, user.DisplayName);
    }

    public async Task<AuthResponseDto> Login(LoginRequestDto request)
    {
        var user = await users.FindByEmailAsync(request.Email?.Trim() ?? "");

        if (user is null || await users.IsLockedOutAsync(user)) throw InvalidCredentials();
        if (!await users.CheckPasswordAsync(user, request.Password ?? ""))
        {
            await users.AccessFailedAsync(user);
            throw InvalidCredentials();
        }

        await users.ResetAccessFailedCountAsync(user);
        var roles = await users.GetRolesAsync(user);
        return tokens.Generate(new CurrentUserDto(user.Id, user.DisplayName, roles.Contains(RoleNames.Moderator)), roles);
    }

    private static ForumException InvalidCredentials() =>
        new("Invalid credentials or account temporarily locked.", ForumError.Unauthorized);
}
