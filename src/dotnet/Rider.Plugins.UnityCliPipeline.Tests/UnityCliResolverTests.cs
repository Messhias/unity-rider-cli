using Xunit;

namespace Rider.Plugins.UnityCliPipeline.Tests;

public class UnityCliResolverTests
{
    [Fact]
    public void Uses_settings_override_when_file_exists()
    {
        var unityPath = CreateFakeUnityBinary();
        var process = new FakeUnityCliProcess(_ => new UnityCliProcessResult(0, "1.0.0-beta.8", ""));
        var resolver = new UnityCliResolver(process, path => path == unityPath);

        var result = resolver.Resolve(new UnityCliSettings { CliPath = unityPath });

        Assert.Equal(unityPath, result.ExecutablePath);
        Assert.Equal("1.0.0-beta.8", result.Version.ToString());
        Assert.Equal(["--version", "--format", "json"], process.LastArgs);
        Assert.Equal(unityPath, process.LastExecutable);
    }

    [Fact]
    public void Resolves_from_path_when_override_missing()
    {
        var unityPath = CreateFakeUnityBinary();
        var process = new FakeUnityCliProcess(_ => new UnityCliProcessResult(0, "1.0.0-beta.8", ""));
        var resolver = new UnityCliResolver(
            process,
            pathExists: _ => false,
            pathLookup: _ => unityPath);

        var result = resolver.Resolve(new UnityCliSettings());

        Assert.Equal(unityPath, result.ExecutablePath);
    }

    [Fact]
    public void Throws_when_cli_cannot_be_found()
    {
        var resolver = new UnityCliResolver(
            new FakeUnityCliProcess(_ => throw new InvalidOperationException("should not run")),
            pathExists: _ => false,
            pathLookup: _ => null);

        var ex = Assert.Throws<UnityCliNotFoundException>(() => resolver.Resolve(new UnityCliSettings()));
        Assert.Contains("unity", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Throws_when_override_path_does_not_exist()
    {
        var resolver = new UnityCliResolver(
            new FakeUnityCliProcess(_ => throw new InvalidOperationException("should not run")),
            pathExists: _ => false);

        var ex = Assert.Throws<UnityCliNotFoundException>(
            () => resolver.Resolve(new UnityCliSettings { CliPath = "/missing/unity" }));
        Assert.Contains("/missing/unity", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Throws_when_version_is_below_minimum()
    {
        var unityPath = CreateFakeUnityBinary();
        var process = new FakeUnityCliProcess(_ => new UnityCliProcessResult(0, "1.0.0-alpha.1", ""));
        var resolver = new UnityCliResolver(process, path => path == unityPath);

        var ex = Assert.Throws<UnityCliVersionTooLowException>(
            () => resolver.Resolve(new UnityCliSettings { CliPath = unityPath }));
        Assert.Contains("1.0.0-beta.1", ex.Message, StringComparison.Ordinal);
        Assert.Contains("1.0.0-alpha.1", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Settings_from_environment_read_cli_path_and_timeout()
    {
        var settings = UnityCliSettings.FromEnvironment(
            name => name switch
            {
                "UNITY_CLI_PATH" => "/opt/unity/bin/unity",
                "UNITY_TEST_TIMEOUT" => "900",
                _ => null
            });

        Assert.Equal("/opt/unity/bin/unity", settings.CliPath);
        Assert.Equal(900, settings.DefaultTimeoutSeconds);
    }

    [Fact]
    public void Settings_from_environment_use_defaults_when_unset()
    {
        var settings = UnityCliSettings.FromEnvironment(_ => null);

        Assert.Null(settings.CliPath);
        Assert.Equal(UnityCliSettings.DefaultTimeoutSecondsValue, settings.DefaultTimeoutSeconds);
    }

    private static string CreateFakeUnityBinary()
    {
        var path = Path.Combine(Path.GetTempPath(), "unity-cli-pipeline-fake-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(path, "fake");
        return path;
    }

    private sealed class FakeUnityCliProcess(Func<IReadOnlyList<string>, UnityCliProcessResult> handler)
        : IUnityCliProcess
    {
        public string? LastExecutable { get; private set; }
        public IReadOnlyList<string>? LastArgs { get; private set; }

        public UnityCliProcessResult Run(string executable, IReadOnlyList<string> arguments)
        {
            LastExecutable = executable;
            LastArgs = arguments;
            return handler(arguments);
        }
    }
}
