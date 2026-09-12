using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers;

[Route("api/v1/health")]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}