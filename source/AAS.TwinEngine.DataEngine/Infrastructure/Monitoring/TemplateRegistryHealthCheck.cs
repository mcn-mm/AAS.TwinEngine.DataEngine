using AAS.TwinEngine.DataEngine.ApplicationLogic.Services.Plugin.Config;
using AAS.TwinEngine.DataEngine.Infrastructure.Http.Clients;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AAS.TwinEngine.DataEngine.Infrastructure.Monitoring;

public sealed class TemplateRegistryHealthCheck(ICreateClient clientFactory,
                                                IOptions<AasEnvironmentConfig> aasEnvironment,
                                                ILogger<TemplateRegistryHealthCheck> logger) : IHealthCheck
{
    private const int HealthCheckTimeoutSeconds = 5;
    private readonly string _aasRegistryPath = aasEnvironment.Value.AasRegistryPath;
    private readonly string _subModelRegistryPath = aasEnvironment.Value.SubModelRegistryPath;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(HealthCheckTimeoutSeconds));

        var aasHealthy = await CheckEndpointAsync(AasEnvironmentConfig.AasRegistryHttpClientName, $"{_aasRegistryPath}?limit=1", "aas-registry", cts.Token).ConfigureAwait(false);

        if (!aasHealthy)
        {
            return HealthCheckResult.Unhealthy();
        }

        var submodelHealthy = await CheckEndpointAsync(AasEnvironmentConfig.SubmodelRegistryHttpClientName, $"{_subModelRegistryPath}?limit=1", "submodel-registry", cts.Token).ConfigureAwait(false);

        return submodelHealthy
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }

    private async Task<bool> CheckEndpointAsync(string clientName, string path, string endpointKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning("Endpoint {EndpointKey} path is not configured", endpointKey);
            return false;
        }

        try
        {
            var httpClient = clientFactory.CreateClient(clientName);
            using var response = await httpClient
                .GetAsync(new Uri(path, UriKind.Relative), cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            logger.LogWarning("Health check failed for {EndpointKey}. Status: {StatusCode}", endpointKey, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Health check failed for {EndpointKey}", endpointKey);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Health check timed out for {EndpointKey}", endpointKey);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Health check failed for {EndpointKey}", endpointKey);
            return false;
        }
    }
}
