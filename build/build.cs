using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Bullseye.Targets;
using static SimpleExec.Command;

var version = GetGitVersion();

Target("clean", () =>
{
    Run("dotnet", "clean --configuration Release");

    if (Directory.Exists("artifacts"))
    {
        Directory.Delete("artifacts", true);
    }

    var filters = Directory.GetFiles(Environment.CurrentDirectory, "*.slnf");

    foreach (var filter in filters)
    {
        File.Delete(filter);
    }
});

Target("restore", DependsOn("clean"), () =>
{
    Run("dotnet", "restore");
});

Target("build", DependsOn("restore"), () =>
{
    var solution = GetSolution();

    Run("dotnet", $"build {solution} " +
                  "--no-restore " +
                  "--configuration Release " +
                  $"/p:Version={version.SemVer} " +
                  $"/p:AssemblyVersion={version.AssemblySemVer} " +
                  $"/p:FileVersion={version.AssemblySemFileVer} " +
                  $"/p:InformationalVersion={version.InformationalVersion}");

    File.Delete(solution);
});

Target("test", DependsOn("build"), () =>
{
    Run("dotnet", "test --configuration Release --no-restore --no-build");
});

Target("package", DependsOn("build", "test"), () =>
{
    Run("dotnet", $"pack --configuration Release --no-restore --no-build --output artifacts /p:Version={version.SemVer}");
});

Target("publish", DependsOn("package"), () =>
{
    var apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

    Run("dotnet", $"nuget push {Path.Combine("artifacts", "*.nupkg")} --api-key {apiKey} --source https://api.nuget.org/v3/index.json");
});

Target("default", DependsOn("package"));

RunTargetsAndExit(args);

static GitVersion GetGitVersion()
{
    Run("dotnet", "tool restore");

    var value = Read("dotnet", "dotnet-gitversion");

    return JsonSerializer.Deserialize<GitVersion>(value);
}

static string GetSolution()
{
    const string name = "Machine.Specifications.Analyzers";

    var solution = new SolutionFilter
    {
        Solution = new Solution
        {
            Path = $"{name}.sln",
            Projects = new[]
            {
                Path.Combine("src", name, $"{name}.csproj"),
                Path.Combine("src", $"{name}.Tests", $"{name}.Tests.csproj")
            }
        }
    };

    File.WriteAllText($"{name}.slnf", JsonSerializer.Serialize(solution));

    return $"{name}.slnf";
}

class SolutionFilter
{
    [JsonPropertyName("solution")]
    public Solution Solution { get; set; }
}

class Solution
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("projects")]
    public string[] Projects { get; set; }
}

class GitVersion
{
    public string SemVer { get; set; }

    public string AssemblySemVer { get; set; }

    public string AssemblySemFileVer { get; set; }

    public string InformationalVersion { get; set; }

    public string PreReleaseTag { get; set; }
}
