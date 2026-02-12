using AAS.TwinEngine.Plugin.TestPlugin.Api.Health;
using AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

namespace AAS.TwinEngine.Plugin.TestPlugin.UnitTests.Api.Health;

public class HealthControllerTests
{
    private readonly IHealthStatusService _healthStatusService = Substitute.For<IHealthStatusService>();
    private readonly HealthController _sut;

    public HealthControllerTests() => _sut = new HealthController(_healthStatusService);

    [Fact]
    public void Get_ShouldReturnOk_WhenServiceIsHealthy()
    {
        _healthStatusService.IsHealthy().Returns(true);

        var result = _sut.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode ?? 200);
    }

    [Fact]
    public void Get_ShouldReturnServiceUnavailable_WhenServiceIsUnhealthy()
    {
        _healthStatusService.IsHealthy().Returns(false);

        var result = _sut.Get();

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }
}
