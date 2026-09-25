using Xunit;

namespace Rider.Plugins.UnityCliPipeline.Tests;

public class UnityCliVersionParserTests
{
    [Theory]
    [InlineData("1.0.0-beta.8")]
    [InlineData("  1.0.0-beta.8\n")]
    public void Parses_plain_version_stdout(string stdout)
    {
        var version = UnityCliVersionParser.Parse(stdout);

        Assert.Equal("1.0.0-beta.8", version.ToString());
        Assert.Equal(1, version.Major);
        Assert.Equal(0, version.Minor);
        Assert.Equal(0, version.Patch);
        Assert.Equal("beta.8", version.Prerelease);
    }

    [Fact]
    public void Parses_json_object_with_version_property()
    {
        var version = UnityCliVersionParser.Parse("""{"version":"1.0.0-beta.8"}""");

        Assert.Equal("1.0.0-beta.8", version.ToString());
    }

    [Fact]
    public void Parses_json_object_with_cliVersion_property()
    {
        var version = UnityCliVersionParser.Parse("""{"success":true,"data":{"cliVersion":"1.0.0-beta.8"}}""");

        Assert.Equal("1.0.0-beta.8", version.ToString());
    }

    [Fact]
    public void Rejects_empty_output()
    {
        Assert.Throws<UnityCliVersionFormatException>(() => UnityCliVersionParser.Parse("   "));
    }

    [Theory]
    [InlineData("1.0.0-beta.8", "1.0.0-beta.1", true)]
    [InlineData("1.0.0-beta.1", "1.0.0-beta.1", true)]
    [InlineData("1.0.0-beta.1", "1.0.0-beta.8", false)]
    [InlineData("1.0.0", "1.0.0-beta.8", true)]
    [InlineData("0.9.0", "1.0.0-beta.1", false)]
    public void Compares_against_minimum(string candidate, string minimum, bool expected)
    {
        var left = UnityCliVersion.Parse(candidate);
        var right = UnityCliVersion.Parse(minimum);

        Assert.Equal(expected, left >= right);
    }
}
