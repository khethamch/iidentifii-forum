using System.Security.Claims;
using Forum.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string? ViewerId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    protected string CurrentUserId => ViewerId
        ?? throw new ForumException("Authentication required.", ForumError.Unauthorized);
}