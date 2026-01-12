using System;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles.GitHub.Http;

public static class GitHubApiUrlBuilder
{
    private const string ApiBase = "https://api.github.com";

    public static Uri BuildContentsUri(string owner, string repository, string path, string? branch = null)
    {
        Guard.NotNullOrWhiteSpace(owner, nameof(owner));
        Guard.NotNullOrWhiteSpace(repository, nameof(repository));
        Guard.NotNullOrWhiteSpace(path, nameof(path));

        var escapedPath = EscapePath(path);
        var uri = $"{ApiBase}/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/contents/{escapedPath}";
        if (!string.IsNullOrWhiteSpace(branch))
        {
            uri += $"?ref={Uri.EscapeDataString(branch)}";
        }

        return new Uri(uri, UriKind.Absolute);
    }

    private static string EscapePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim('/');
        var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("/", Array.ConvertAll(segments, Uri.EscapeDataString));
    }
}
