using System.Text.RegularExpressions;

namespace Rider.Plugins.UnityCliPipeline;

/// <summary>
/// Editor support policy: current Unity LTS tracks only (final releases).
/// Tech Stream, alpha/beta Editor builds, and older LTS lines are out of scope for now.
/// </summary>
public static partial class UnityEditorLtsSupport
{
    /// <summary>
    /// Supported LTS major, minor tracks (Unity 6 LTS + 2022.3 LTS).
    /// </summary>
    private static IReadOnlyList<string> SupportedTracks { get; } = ["6000.0", "2022.3"];

    private static readonly Regex EditorVersionPattern = MyRegex();

    public static bool IsSupported(string? editorVersion)
    {
        if (string.IsNullOrWhiteSpace(editorVersion))
            return false;

        var match = EditorVersionPattern.Match(editorVersion.Trim());
        if (!match.Success)
            return false;

        // Final releases only (f). Reject a/b/rc/etc.
        if (!string.Equals(match.Groups["stream"].Value, "f", StringComparison.OrdinalIgnoreCase))
            return false;

        var track = $"{match.Groups["major"].Value}.{match.Groups["minor"].Value}";
        return SupportedTracks.Contains(track, StringComparer.Ordinal);
    }

    [GeneratedRegex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<stream>[a-zA-Z]+)(?<build>\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
}
