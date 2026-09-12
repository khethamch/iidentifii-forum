using Forum.Application.Dtos.Auth;
using Forum.Domain.Entities;
using Forum.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Forum.Api.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType<RegisterResponseDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await auth.Register(request);
        return CreatedAtAction(nameof(Me), result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request) =>
        Ok(await auth.Login(request));

    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me() =>
        Ok(new CurrentUserDto(CurrentUserId, User.Identity!.Name!, User.IsInRole(RoleNames.Moderator)));
}
