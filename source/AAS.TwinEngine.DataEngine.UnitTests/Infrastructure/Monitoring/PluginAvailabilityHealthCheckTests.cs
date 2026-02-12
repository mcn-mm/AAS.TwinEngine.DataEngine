using System.Net;

using AAS.TwinEngine.DataEngine.Infrastructure.Http.Clients;
using AAS.TwinEngine.DataEngine.Infrastructure.Monitoring;
using AAS.TwinEngine.DataEngine.Infrastructure.Providers.PluginDataProvider.Config;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace AAS.TwinEngine.DataEngine.UnitTests.Infrastructure.Monitoring;

public class PluginAvailabilityHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_PluginManifest_Is_Unhealthy()
    {
        var clientFactory = Substitute.For<ICreateClient>();

        var pluginConfig = Options.Create(new PluginConfig
        {
            Plugins =
            [
                new Plugin
                {
                    PluginName = "TestPlugin",
                    PluginUrl = new Uri("http://localhost")
                }
            ]
        });

        var pluginManifestHealthStatus = Substitute.For<IPluginManifestHealthStatus>();
        pluginManifestHealthStatus.IsHealthy.Returns(false);

        var logger = Substitute.For<ILogger<PluginAvailabilityHealthCheck>>();

        var sut = new PluginAvailabilityHealthCheck(clientFactory, pluginConfig, pluginManifestHealthStatus, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_No_Plugins_Configured()
    {
        var clientFactory = Substitute.For<ICreateClient>();

        var pluginConfig = Options.Create(new PluginConfig
        {
            Plugins = []
        });

        var pluginManifestHealthStatus = Substitute.For<IPluginManifestHealthStatus>();
        pluginManifestHealthStatus.IsHealthy.Returns(true);

        var logger = Substitute.For<ILogger<PluginAvailabilityHealthCheck>>();

        var sut = new PluginAvailabilityHealthCheck(clientFactory, pluginConfig, pluginManifestHealthStatus, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Healthy_When_All_Plugins_Are_Healthy()
    {
        var clientFactory = Substitute.For<ICreateClient>();

        clientFactory
            .CreateClient(Arg.Any<string>())
            .Returns(callInfo =>
            {
                var handler = new StubHttpMessageHandler((_, _) =>
                    Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri("http://localhost")
                };
            });

        var pluginConfig = Options.Create(new PluginConfig
        {
            Plugins =
            [
                new Plugin { PluginName = "Plugin1", PluginUrl = new Uri("http://localhost") },
                new Plugin { PluginName = "Plugin2", PluginUrl = new Uri("http://localhost") }
            ]
        });

        var pluginManifestHealthStatus = Substitute.For<IPluginManifestHealthStatus>();
        pluginManifestHealthStatus.IsHealthy.Returns(true);

        var logger = Substitute.For<ILogger<PluginAvailabilityHealthCheck>>();

        var sut = new PluginAvailabilityHealthCheck(clientFactory, pluginConfig, pluginManifestHealthStatus, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_Any_Plugin_Is_Unhealthy()
    {
        var clientFactory = Substitute.For<ICreateClient>();

        clientFactory
            .CreateClient(Arg.Any<string>())
            .Returns(callInfo =>
            {
                var handler = new StubHttpMessageHandler((_, _) =>
                    Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri("http://localhost")
                };
            });

        var pluginConfig = Options.Create(new PluginConfig
        {
            Plugins =
            [
                new Plugin { PluginName = "Plugin1", PluginUrl = new Uri("http://localhost") }
            ]
        });

        var pluginManifestHealthStatus = Substitute.For<IPluginManifestHealthStatus>();
        pluginManifestHealthStatus.IsHealthy.Returns(true);

        var logger = Substitute.For<ILogger<PluginAvailabilityHealthCheck>>();

        var sut = new PluginAvailabilityHealthCheck(clientFactory, pluginConfig, pluginManifestHealthStatus, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}
