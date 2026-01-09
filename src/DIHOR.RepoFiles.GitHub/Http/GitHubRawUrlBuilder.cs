using System;

namespace DIHOR.RepoFiles.GitHub.Http;

public static class GitHubRawUrlBuilder
{
    public static Uri BuildRawFileUri(string owner, string repository, string branch, string path)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            throw new ArgumentException("Owner cannot be null or whitespace.", nameof(owner));
        }

        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new ArgumentException("Repository cannot be null or whitespace.", nameof(repository));
        }

        if (string.IsNullOrWhiteSpace(branch))
        {
            throw new ArgumentException("Branch cannot be null or whitespace.", nameof(branch));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        }

        var normalizedPath = path.Replace('\\', '/').TrimStart('/');
        var url = $"https://raw.githubusercontent.com/{owner}/{repository}/{branch}/{normalizedPath}";
        return new Uri(url, UriKind.Absolute);
    }
}
