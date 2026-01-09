using DIHOR.RepoFiles;

namespace DIHOR.RepoFiles.Configuration;

public sealed class RepoFilesConfigurationOptions
{
    public string Provider { get; set; } = string.Empty;

    public GitHubOptions GitHub { get; set; } = new GitHubOptions();

    public DownloadOptions Download { get; set; } = new DownloadOptions();
}

public sealed class GitHubOptions
{
    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string Branch { get; set; } = "main";

    public string ManifestPath { get; set; } = "manifest.json";
}
