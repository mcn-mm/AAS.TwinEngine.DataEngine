using System.Net;
using System.Net.Http;

using AAS.TwinEngine.DataEngine.ApplicationLogic.Services.Plugin.Config;
using AAS.TwinEngine.DataEngine.Infrastructure.Http.Clients;
using AAS.TwinEngine.DataEngine.Infrastructure.Monitoring;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace AAS.TwinEngine.DataEngine.UnitTests.Infrastructure.Monitoring;

public class TemplateRegistryHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_Returns_Healthy_When_Registry_And_Submodel_Are_Healthy()
    {
        var environmentConfig = new AasEnvironmentConfig
        {
            AasRegistryPath = "/aas",
            SubModelRegistryPath = "/submodel"
        };

        var options = Options.Create(environmentConfig);

        var clientFactory = Substitute.For<ICreateClient>();

        var aasClient = CreateHttpClient(HttpStatusCode.OK);
        var submodelClient = CreateHttpClient(HttpStatusCode.OK);

        clientFactory.CreateClient(AasEnvironmentConfig.AasRegistryHttpClientName).Returns(aasClient);
        clientFactory.CreateClient(AasEnvironmentConfig.SubmodelRegistryHttpClientName).Returns(submodelClient);

        var logger = Substitute.For<ILogger<TemplateRegistryHealthCheck>>();

        var sut = new TemplateRegistryHealthCheck(clientFactory, options, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_AasRegistry_Is_Unhealthy()
    {
        var environmentConfig = new AasEnvironmentConfig
        {
            AasRegistryPath = "/aas",
            SubModelRegistryPath = "/submodel"
        };

        var options = Options.Create(environmentConfig);

        var clientFactory = Substitute.For<ICreateClient>();

        var aasClient = CreateHttpClient(HttpStatusCode.InternalServerError);
        var submodelClient = CreateHttpClient(HttpStatusCode.OK);

        clientFactory.CreateClient(AasEnvironmentConfig.AasRegistryHttpClientName).Returns(aasClient);
        clientFactory.CreateClient(AasEnvironmentConfig.SubmodelRegistryHttpClientName).Returns(submodelClient);

        var logger = Substitute.For<ILogger<TemplateRegistryHealthCheck>>();

        var sut = new TemplateRegistryHealthCheck(clientFactory, options, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_SubmodelRegistry_Is_Unhealthy()
    {
        var environmentConfig = new AasEnvironmentConfig
        {
            AasRegistryPath = "/aas",
            SubModelRegistryPath = "/submodel"
        };

        var options = Options.Create(environmentConfig);

        var clientFactory = Substitute.For<ICreateClient>();

        var aasClient = CreateHttpClient(HttpStatusCode.OK);
        var submodelClient = CreateHttpClient(HttpStatusCode.InternalServerError);

        clientFactory.CreateClient(AasEnvironmentConfig.AasRegistryHttpClientName).Returns(aasClient);
        clientFactory.CreateClient(AasEnvironmentConfig.SubmodelRegistryHttpClientName).Returns(submodelClient);

        var logger = Substitute.For<ILogger<TemplateRegistryHealthCheck>>();

        var sut = new TemplateRegistryHealthCheck(clientFactory, options, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Returns_Unhealthy_When_AasRegistry_Path_Is_Not_Configured()
    {
        var environmentConfig = new AasEnvironmentConfig
        {
            AasRegistryPath = string.Empty,
            SubModelRegistryPath = "/submodel"
        };

        var options = Options.Create(environmentConfig);

        var clientFactory = Substitute.For<ICreateClient>();

        var logger = Substitute.For<ILogger<TemplateRegistryHealthCheck>>();

        var sut = new TemplateRegistryHealthCheck(clientFactory, options, logger);

        var result = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode)
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode)));

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}
