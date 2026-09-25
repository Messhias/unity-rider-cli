using System.Text.Json;

namespace Rider.Plugins.UnityCliPipeline;

public static class UnityCliVersionParser
{
    public static UnityCliVersion Parse(string stdout)
    {
        if (string.IsNullOrWhiteSpace(stdout))
            throw new UnityCliVersionFormatException("Unity CLI version output is empty.");

        var trimmed = stdout.Trim();

        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if (TryReadVersion(document.RootElement, out var fromJson) && fromJson is not null)
                    return fromJson;
            }
            catch (JsonException ex)
            {
                throw new UnityCliVersionFormatException("Unity CLI version JSON could not be parsed.", ex);
            }

            throw new UnityCliVersionFormatException("Unity CLI version JSON did not contain a version field.");
        }

        // `unity --version --format json` currently prints a plain semver string.
        var firstLine = trimmed.Split('\n', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
        return UnityCliVersion.Parse(firstLine);
    }

    private static bool TryReadVersion(JsonElement element, out UnityCliVersion? version)
    {
        version = null;

        if (element.ValueKind != JsonValueKind.Object) return false;

        foreach (var name in new[] { "version", "cliVersion", "cli_version" })
        {
            if (element.TryGetProperty(name, out var property)
                && property.ValueKind == JsonValueKind.String
                && UnityCliVersion.TryParse(property.GetString(), out version))
            {
                return true;
            }
        }

        return element.TryGetProperty("data", out var data) && TryReadVersion(data, out version);
    }
}
