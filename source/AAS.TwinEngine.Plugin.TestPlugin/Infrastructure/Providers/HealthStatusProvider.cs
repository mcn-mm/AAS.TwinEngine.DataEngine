using AAS.TwinEngine.Plugin.TestPlugin.ApplicationLogic.Services.Health;

namespace AAS.TwinEngine.Plugin.TestPlugin.Infrastructure.Providers;

public class HealthStatusProvider(IHostEnvironment hostEnvironment) : IHealthStatusProvider
{
    private const string MetadataFileName = "mock-metadata.json";
    private const string DataFileName = "mock-submodel-data.json";

    public bool IsHealthy()
    {
        var dataFolder = Path.Combine(hostEnvironment.ContentRootPath, "Data");

        var metadataPath = Path.Combine(dataFolder, MetadataFileName);
        var dataPath = Path.Combine(dataFolder, DataFileName);

        var metadataExists = File.Exists(metadataPath);
        var dataExists = File.Exists(dataPath);

        return metadataExists && dataExists;
    }
}
