using AAS.TwinEngine.Plugin.TestPlugin.Api.Manifest.Responses;
using AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

using Microsoft.AspNetCore.Mvc;

namespace AAS.TwinEngine.Plugin.TestPlugin.Api.Health;

[ApiController]
[Route("[controller]")]
public class HealthController(IHealthStatusService healthStatusService) : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Get()
    {
        var isHealthy = healthStatusService.IsHealthy();

        if (isHealthy)
        {
            return Ok(new HealthResponseDto
            {
                Status = "Healthy"
            });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable,
            new HealthResponseDto
            {
                Status = "Unhealthy"
            });
    }
}
