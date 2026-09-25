namespace Rider.Plugins.UnityCliPipeline;

public sealed class UnityCliVersion : IComparable<UnityCliVersion>
{
    private UnityCliVersion(int major, int minor, int patch, string? prerelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = string.IsNullOrWhiteSpace(prerelease) ? null : prerelease;
    }

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Prerelease { get; }

    public static UnityCliVersion Parse(string text)
    {
        if (!TryParse(text, out var version) || version is null)
            throw new UnityCliVersionFormatException($"Invalid Unity CLI version: '{text}'.");

        return version;
    }

    public static bool TryParse(string? text, out UnityCliVersion? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var value = text.Trim();
        var dash = value.IndexOf('-');
        var core = dash >= 0 ? value[..dash] : value;
        var pre = dash >= 0 ? value[(dash + 1)..] : null;

        var parts = core.Split('.');
        if (parts.Length != 3
            || !int.TryParse(parts[0], out var major)
            || !int.TryParse(parts[1], out var minor)
            || !int.TryParse(parts[2], out var patch))
        {
            return false;
        }

        version = new UnityCliVersion(major, minor, patch, pre);
        return true;
    }

    public int CompareTo(UnityCliVersion? other)
    {
        if (other is null)
            return 1;

        var core = Major.CompareTo(other.Major);
        if (core != 0) return core;
        core = Minor.CompareTo(other.Minor);
        if (core != 0) return core;
        core = Patch.CompareTo(other.Patch);
        if (core != 0) return core;

        // No prerelease is greater than any prerelease (1.0.0 > 1.0.0-beta.8).
        switch (Prerelease)
        {
            case null when other.Prerelease is null:
                return 0;
            case null:
                return 1;
        }

        if (other.Prerelease is null) return -1;

        return ComparePrerelease(Prerelease, other.Prerelease);
    }

    public static bool operator >=(UnityCliVersion left, UnityCliVersion right) => left.CompareTo(right) >= 0;
    public static bool operator <=(UnityCliVersion left, UnityCliVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >(UnityCliVersion left, UnityCliVersion right) => left.CompareTo(right) > 0;
    public static bool operator <(UnityCliVersion left, UnityCliVersion right) => left.CompareTo(right) < 0;

    public override string ToString() =>
        Prerelease is null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}-{Prerelease}";

    private static int ComparePrerelease(string left, string right)
    {
        var leftParts = left.Split('.');
        var rightParts = right.Split('.');
        var length = Math.Max(leftParts.Length, rightParts.Length);

        for (var i = 0; i < length; i++)
        {
            if (i >= leftParts.Length) return -1;
            if (i >= rightParts.Length) return 1;

            var l = leftParts[i];
            var r = rightParts[i];
            var lNum = int.TryParse(l, out var ln);
            var rNum = int.TryParse(r, out var rn);

            if (lNum && rNum)
            {
                var cmp = ln.CompareTo(rn);
                if (cmp != 0) return cmp;
                continue;
            }

            if (lNum != rNum)
                return lNum ? -1 : 1;

            var textCmp = string.CompareOrdinal(l, r);
            if (textCmp != 0) return textCmp;
        }

        return 0;
    }
}
