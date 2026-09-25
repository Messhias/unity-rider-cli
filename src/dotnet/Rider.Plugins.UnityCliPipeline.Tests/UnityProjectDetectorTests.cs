using Xunit;

namespace Rider.Plugins.UnityCliPipeline.Tests;

public class UnityProjectDetectorTests
{
    private static string TestDataRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "testData");
                if (Directory.Exists(candidate) && File.Exists(Path.Combine(dir.FullName, "Tasks.md")))
                    return candidate;
                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException(
                $"Could not locate testData from {AppContext.BaseDirectory}");
        }
    }

    private static string UnityFixture => Path.Combine(TestDataRoot, "unity", "MinimalUnityProject");
    private static string DotNetFixture => Path.Combine(TestDataRoot, "dotnet", "PlainDotNet");

    [Fact]
    public void Detects_unity_root_from_solution_at_project_root()
    {
        var solutionPath = Path.Combine(UnityFixture, "MinimalUnityProject.sln");

        var info = UnityProjectDetector.TryDetectFromSolutionPath(solutionPath);

        Assert.NotNull(info);
        Assert.Equal(Path.GetFullPath(UnityFixture), info.Path);
        Assert.Equal("6000.0.0f1", info.EditorVersion);
    }

    [Fact]
    public void Detects_unity_root_when_solution_is_in_a_subdirectory()
    {
        var solutionPath = Path.Combine(UnityFixture, "nested", "Nested.sln");

        var info = UnityProjectDetector.TryDetectFromSolutionPath(solutionPath);

        Assert.NotNull(info);
        Assert.Equal(Path.GetFullPath(UnityFixture), info.Path);
        Assert.Equal("6000.0.0f1", info.EditorVersion);
    }

    [Fact]
    public void Detects_unity_root_from_directory_path()
    {
        var info = UnityProjectDetector.TryDetectFromSolutionPath(UnityFixture);

        Assert.NotNull(info);
        Assert.Equal(Path.GetFullPath(UnityFixture), info.Path);
    }

    [Fact]
    public void Returns_null_for_plain_dotnet_solution()
    {
        var solutionPath = Path.Combine(DotNetFixture, "PlainDotNet.sln");

        var info = UnityProjectDetector.TryDetectFromSolutionPath(solutionPath);

        Assert.Null(info);
    }

    [Fact]
    public void Returns_null_when_unity_markers_are_incomplete()
    {
        var incomplete = Path.Combine(TestDataRoot, "unity", "IncompleteUnityProject");

        var info = UnityProjectDetector.TryDetectFromSolutionPath(incomplete);

        Assert.Null(info);
    }

    [Fact]
    public void Returns_null_for_missing_path()
    {
        var info = UnityProjectDetector.TryDetectFromSolutionPath(
            Path.Combine(Path.GetTempPath(), "unity-cli-pipeline-does-not-exist-" + Guid.NewGuid()));

        Assert.Null(info);
    }

    [Fact]
    public void Returns_null_for_non_lts_editor_version()
    {
        var root = CreateTempUnityProject("6000.1.0f1");
        try
        {
            var info = UnityProjectDetector.TryDetectFromSolutionPath(root);
            Assert.Null(info);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempUnityProject(string editorVersion)
    {
        var root = Path.Combine(Path.GetTempPath(), "unity-cli-pipeline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Assets"));
        Directory.CreateDirectory(Path.Combine(root, "Packages"));
        Directory.CreateDirectory(Path.Combine(root, "ProjectSettings"));
        File.WriteAllText(Path.Combine(root, "Packages", "manifest.json"), "{}");
        File.WriteAllText(
            Path.Combine(root, "ProjectSettings", "ProjectVersion.txt"),
            $"m_EditorVersion: {editorVersion}\n");
        return root;
    }
}
