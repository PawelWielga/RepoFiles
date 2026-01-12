namespace DIHOR.RepoFiles.GitHub;

public sealed class GitHubPublisherOptions
{
    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string Branch { get; set; } = "main";

    public string Token { get; set; } = string.Empty;

    public string? CommitterName { get; set; }

    public string? CommitterEmail { get; set; }
}
