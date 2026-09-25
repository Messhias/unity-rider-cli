namespace Rider.Plugins.UnityCliPipeline;

/// <summary>
/// Detected Unity project root and the Editor version from ProjectVersion.txt.
/// </summary>
public sealed class UnityProjectInfo(string path, string editorVersion)
{
    public string Path { get; } = path;
    public string EditorVersion { get; } = editorVersion;
}
