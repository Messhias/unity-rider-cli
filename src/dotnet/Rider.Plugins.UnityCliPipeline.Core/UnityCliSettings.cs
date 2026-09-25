namespace Rider.Plugins.UnityCliPipeline;

public sealed class UnityCliSettings
{
    public const int DefaultTimeoutSecondsValue = 600;
    private const string CliPathEnvironmentVariable = "UNITY_CLI_PATH";
    private const string TimeoutEnvironmentVariable = "UNITY_TEST_TIMEOUT";

    public string? CliPath { get; init; }
    public int DefaultTimeoutSeconds { get; private init; } = DefaultTimeoutSecondsValue;

    public static UnityCliSettings FromEnvironment(Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

        var cliPath = getEnvironmentVariable(CliPathEnvironmentVariable);
        var timeoutRaw = getEnvironmentVariable(TimeoutEnvironmentVariable);
        var timeout = DefaultTimeoutSecondsValue;
        if (!string.IsNullOrWhiteSpace(timeoutRaw) && int.TryParse(timeoutRaw, out var parsed) && parsed > 0)
            timeout = parsed;

        return new UnityCliSettings
        {
            CliPath = string.IsNullOrWhiteSpace(cliPath) ? null : cliPath.Trim(),
            DefaultTimeoutSeconds = timeout
        };
    }
}
