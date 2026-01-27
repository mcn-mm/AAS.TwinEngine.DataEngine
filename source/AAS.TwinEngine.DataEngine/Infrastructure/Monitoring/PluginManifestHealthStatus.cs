namespace AAS.TwinEngine.DataEngine.Infrastructure.Monitoring;

public sealed class PluginManifestHealthStatus : IPluginManifestHealthStatus
{
    private int _isHealthy = 1;

    public bool IsHealthy
    {
        get => Interlocked.CompareExchange(ref _isHealthy, 1, 1) == 1;
        set => Interlocked.Exchange(ref _isHealthy, value ? 1 : 0);
    }
}
