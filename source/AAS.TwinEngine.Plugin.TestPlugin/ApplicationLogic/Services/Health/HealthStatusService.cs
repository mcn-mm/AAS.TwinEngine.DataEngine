namespace AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

public class HealthStatusService(IHealthStatusProvider provider) : IHealthStatusService
{
    public bool IsHealthy() => provider.IsHealthy();
}
