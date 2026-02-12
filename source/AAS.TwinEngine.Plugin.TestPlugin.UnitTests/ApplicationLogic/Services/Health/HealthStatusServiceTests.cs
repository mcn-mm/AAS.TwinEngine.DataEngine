using AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

using NSubstitute;

namespace AAS.TwinEngine.Plugin.TestPlugin.UnitTests.ApplicationLogic.Services.Health;

public class HealthStatusServiceTests
{
    private readonly IHealthStatusProvider _provider = Substitute.For<IHealthStatusProvider>();
    private readonly HealthStatusService _sut;

    public HealthStatusServiceTests() => _sut = new HealthStatusService(_provider);

    [Fact]
    public void IsHealthy_ShouldReturnTrue_WhenProviderIsHealthy()
    {
        _provider.IsHealthy().Returns(true);

        var result = _sut.IsHealthy();

        Assert.True(result);
    }

    [Fact]
    public void IsHealthy_ShouldReturnFalse_WhenProviderIsUnhealthy()
    {
        _provider.IsHealthy().Returns(false);

        var result = _sut.IsHealthy();

        Assert.False(result);
    }
}
