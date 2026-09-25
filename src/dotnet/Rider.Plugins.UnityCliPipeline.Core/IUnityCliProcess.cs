namespace Rider.Plugins.UnityCliPipeline;

public interface IUnityCliProcess
{
    UnityCliProcessResult Run(string executable, IReadOnlyList<string> arguments);
}

public sealed class UnityCliProcessResult(int exitCode, string standardOutput, string standardError)
{
    public int ExitCode { get; } = exitCode;
    public string StandardOutput { get; } = standardOutput;
    public string StandardError { get; } = standardError;
}
