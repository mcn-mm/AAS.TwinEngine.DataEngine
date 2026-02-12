using AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

using Microsoft.AspNetCore.Mvc;

using Asp.Versioning;

namespace AAS.TwinEngine.Plugin.TestPlugin.Api.Health;

[ApiController]
[Route("")]
[ApiVersion(1)]
public class HealthController(IHealthStatusService healthStatusService) : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Get()
    {
        var isHealthy = healthStatusService.IsHealthy();

        if (isHealthy)
        {
            return Ok(new { status = "Healthy" });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "Unhealthy" });
    }
}
