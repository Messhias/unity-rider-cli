namespace Rider.Plugins.UnityCliPipeline;

/// <summary>
/// Locates a Unity project root from a Rider solution path (file or directory).
/// </summary>
public static class UnityProjectDetector
{
    private const string AssetsFolder = "Assets";
    private const string PackagesManifest = "Packages/manifest.json";
    private const string ProjectVersionFile = "ProjectSettings/ProjectVersion.txt";
    private const string EditorVersionPrefix = "m_EditorVersion:";

    public static UnityProjectInfo? TryDetectFromSolutionPath(string solutionPath)
    {
        if (string.IsNullOrWhiteSpace(solutionPath))
            return null;

        var startDirectory = ResolveStartDirectory(solutionPath);
        if (startDirectory is null)
            return null;

        for (var current = new DirectoryInfo(startDirectory);
             current != null;
             current = current.Parent)
        {
            if (!IsUnityProjectRoot(current.FullName))
                continue;

            var editorVersion = TryReadEditorVersion(current.FullName);
            if (editorVersion is null)
                return null;

            // Provider stays inactive for non-LTS / prerelease Editor projects.
            return !UnityEditorLtsSupport.IsSupported(editorVersion) ? null : new UnityProjectInfo(Path.GetFullPath(current.FullName), editorVersion);
        }

        return null;
    }

    private static bool IsUnityProjectRoot(string directory)
    {
        return Directory.Exists(Path.Combine(directory, AssetsFolder))
               && File.Exists(Path.Combine(directory, PackagesManifest))
               && File.Exists(Path.Combine(directory, ProjectVersionFile));
    }

    private static string? TryReadEditorVersion(string unityProjectRoot) =>
        !File.Exists(Path.Combine(unityProjectRoot, ProjectVersionFile))
            ? null
            : (from line in File.ReadLines(Path.Combine(unityProjectRoot, ProjectVersionFile))
                select line.Trim()
                into trimmed
                where trimmed.StartsWith(EditorVersionPrefix, StringComparison.Ordinal)
                select trimmed[EditorVersionPrefix.Length..].Trim()
                into value
                select string.IsNullOrEmpty(value) ? null : value).FirstOrDefault();

    private static string? ResolveStartDirectory(string solutionPath)
    {
        if (File.Exists(solutionPath))
            return Path.GetDirectoryName(Path.GetFullPath(solutionPath));

        return Directory.Exists(solutionPath) ? Path.GetFullPath(solutionPath) : null;
    }
}
