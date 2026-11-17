using Microsoft.AspNetCore.Mvc;

namespace FloorPlanner.Api.Controllers;

/// <summary>Reports that the API is running.</summary>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
