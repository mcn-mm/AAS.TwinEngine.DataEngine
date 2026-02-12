using AAS.TwinEngine.Plugin.TestPlugin.Infrastructure.Providers;

using Microsoft.Extensions.Hosting;

using NSubstitute;

namespace AAS.TwinEngine.Plugin.TestPlugin.UnitTests.Infrastructure.Providers;

public class HealthStatusProviderTests
{
    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();

    [Fact]
    public void IsHealthy_ShouldReturnTrue_WhenBothFilesExist()
    {
        using var tempDir = new TempDirectory();

        _hostEnvironment.ContentRootPath.Returns(tempDir.Path);

        var dataFolder = Path.Combine(tempDir.Path, "Data");
        Directory.CreateDirectory(dataFolder);
        File.WriteAllText(Path.Combine(dataFolder, "mock-metadata.json"), "{}");
        File.WriteAllText(Path.Combine(dataFolder, "mock-submodel-data.json"), "{}");

        var sut = new HealthStatusProvider(_hostEnvironment);

        var result = sut.IsHealthy();

        Assert.True(result);
    }

    [Fact]
    public void IsHealthy_ShouldReturnFalse_WhenAnyFileIsMissing()
    {
        using var tempDir = new TempDirectory();

        _hostEnvironment.ContentRootPath.Returns(tempDir.Path);

        var dataFolder = Path.Combine(tempDir.Path, "Data");
        Directory.CreateDirectory(dataFolder);
        File.WriteAllText(Path.Combine(dataFolder, "mock-metadata.json"), "{}");

        var sut = new HealthStatusProvider(_hostEnvironment);

        var result = sut.IsHealthy();

        Assert.False(result);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
