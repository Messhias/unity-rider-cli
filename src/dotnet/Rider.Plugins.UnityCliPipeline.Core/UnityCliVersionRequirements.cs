namespace Rider.Plugins.UnityCliPipeline;

/// <summary>
/// Unity CLI support floor.
/// The CLI is still on a public beta line; there is no stable/LTS CLI release yet.
/// When Unity ships a stable CLI, raise this to that version and drop prerelease support.
/// </summary>
public static class UnityCliVersionRequirements
{
    public static UnityCliVersion MinimumSupported { get; } = UnityCliVersion.Parse("1.0.0-beta.1");
}
