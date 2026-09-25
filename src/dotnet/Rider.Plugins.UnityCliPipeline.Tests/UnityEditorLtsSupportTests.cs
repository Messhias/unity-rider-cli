using Xunit;

namespace Rider.Plugins.UnityCliPipeline.Tests;

public class UnityEditorLtsSupportTests
{
    [Theory]
    [InlineData("6000.0.0f1")]
    [InlineData("6000.0.23f1")]
    [InlineData("2022.3.10f1")]
    public void Accepts_current_lts_final_releases(string version)
    {
        Assert.True(UnityEditorLtsSupport.IsSupported(version));
    }

    [Theory]
    [InlineData("6000.0.0b12")]
    [InlineData("6000.0.0a1")]
    [InlineData("6000.1.0f1")]
    [InlineData("2023.2.0f1")]
    [InlineData("2021.3.40f1")]
    [InlineData("not-a-version")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_beta_tech_stream_and_outdated(string? version)
    {
        Assert.False(UnityEditorLtsSupport.IsSupported(version));
    }
}
