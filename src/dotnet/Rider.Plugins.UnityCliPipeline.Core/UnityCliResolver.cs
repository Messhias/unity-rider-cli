namespace Rider.Plugins.UnityCliPipeline;

public sealed class UnityCliResolution(string executablePath, UnityCliVersion version)
{
    public string ExecutablePath { get; } = executablePath;
    public UnityCliVersion Version { get; } = version;
}

public sealed class UnityCliResolver(
    IUnityCliProcess process,
    Func<string, bool>? pathExists = null,
    Func<string, string?>? pathLookup = null)
{
    private readonly Func<string, bool> _pathExists = pathExists ?? File.Exists;
    private readonly Func<string, string?> _pathLookup = pathLookup ?? FindOnPath;

    public UnityCliResolution Resolve(UnityCliSettings settings)
    {
        var executable = ResolveExecutablePath(settings);
        var probe = process.Run(executable, ["--version", "--format", "json"]);
        if (probe.ExitCode != 0)
        {
            throw new UnityCliVersionFormatException(
                $"Unity CLI version probe failed with exit code {probe.ExitCode}: {probe.StandardError}".Trim());
        }

        var version = UnityCliVersionParser.Parse(probe.StandardOutput);
        if (version < UnityCliVersionRequirements.MinimumSupported)
        {
            throw new UnityCliVersionTooLowException(
                $"Unity CLI {version} is below the minimum supported version {UnityCliVersionRequirements.MinimumSupported}.");
        }

        return new UnityCliResolution(executable, version);
    }

    private string ResolveExecutablePath(UnityCliSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.CliPath))
        {
            if (!_pathExists(settings.CliPath))
            {
                throw new UnityCliNotFoundException(
                    $"Unity CLI override path was not found: '{settings.CliPath}'.");
            }

            return settings.CliPath;
        }

        var fromPath = _pathLookup("unity") ?? (OperatingSystem.IsWindows() ? _pathLookup("unity.exe") : null);
        if (fromPath is null)
        {
            throw new UnityCliNotFoundException(
                "Unity CLI was not found on PATH. Install the Unity CLI or set UNITY_CLI_PATH / the plugin CLI path setting.");
        }

        return fromPath;
    }

    private static string? FindOnPath(string fileName)
    {
        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PATH"))
            ? null
            : (from directory in Environment.GetEnvironmentVariable("PATH")
                    ?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                select Path.Combine(directory.Trim('"'), fileName)
                into candidate
                where File.Exists(candidate)
                select Path.GetFullPath(candidate)).FirstOrDefault();
    }
}
