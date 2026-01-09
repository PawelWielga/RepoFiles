namespace DIHOR.RepoFiles.GitHub;

public sealed class GitHubRepositoryOptions
{
    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string Branch { get; set; } = "main";

    public string ManifestPath { get; set; } = "manifest.json";
}
