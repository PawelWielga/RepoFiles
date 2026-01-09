using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.GitHub.Http;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles.GitHub.Providers;

public sealed class GitHubRepositoryProvider : IRepositoryProvider, IDisposable
{
    private readonly GitHubRepositoryOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeClient;

    public GitHubRepositoryProvider(GitHubRepositoryOptions options, HttpClient? httpClient = null)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNullOrWhiteSpace(options.Owner, nameof(options.Owner));
        Guard.NotNullOrWhiteSpace(options.Repository, nameof(options.Repository));
        Guard.NotNullOrWhiteSpace(options.Branch, nameof(options.Branch));
        Guard.NotNullOrWhiteSpace(options.ManifestPath, nameof(options.ManifestPath));

        _options = options;
        _httpClient = httpClient ?? new HttpClient();
        _disposeClient = httpClient is null;
    }

    public GitHubRepositoryProvider(string owner, string repository, string branch, string manifestPath, HttpClient? httpClient = null)
        : this(new GitHubRepositoryOptions
        {
            Owner = owner,
            Repository = repository,
            Branch = branch,
            ManifestPath = manifestPath
        }, httpClient)
    {
    }

    public Task<Stream> OpenManifestStreamAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var uri = GitHubRawUrlBuilder.BuildRawFileUri(_options.Owner, _options.Repository, _options.Branch, _options.ManifestPath);
        return _httpClient.GetStreamAsync(uri);
    }

    public Task<Stream> OpenFileStreamAsync(ManifestEntry entry, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entry, nameof(entry));
        cancellationToken.ThrowIfCancellationRequested();

        Uri uri;
        if (!string.IsNullOrWhiteSpace(entry.Url))
        {
            if (Uri.TryCreate(entry.Url, UriKind.Absolute, out var absolute))
            {
                uri = absolute;
            }
            else
            {
                var rawPath = entry.Url!;
                uri = GitHubRawUrlBuilder.BuildRawFileUri(_options.Owner, _options.Repository, _options.Branch, rawPath);
            }
        }
        else
        {
            Guard.NotNullOrWhiteSpace(entry.Filename, nameof(entry.Filename));
            uri = GitHubRawUrlBuilder.BuildRawFileUri(_options.Owner, _options.Repository, _options.Branch, entry.Filename);
        }

        return _httpClient.GetStreamAsync(uri);
    }

    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient.Dispose();
        }
    }
}
