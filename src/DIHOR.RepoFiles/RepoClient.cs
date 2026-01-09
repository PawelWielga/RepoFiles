using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Services;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles;

public sealed class RepoClient : IRepositoryClient
{
    private readonly IRepositoryProvider _provider;
    private readonly RepoFilesOptions _options;
    private readonly ManifestService _manifestService;
    private readonly DownloadService _downloadService;

    public RepoClient(IRepositoryProvider provider, RepoFilesOptions? options = null)
    {
        Guard.NotNull(provider, nameof(provider));
        _provider = provider;
        _options = options ?? new RepoFilesOptions();
        _manifestService = new ManifestService(provider);
        _downloadService = new DownloadService(provider, new UpdateDecisionService());
    }

    public Task<IReadOnlyList<ManifestEntry>> GetManifestAsync(CancellationToken cancellationToken = default)
    {
        return _manifestService.GetManifestAsync(cancellationToken);
    }

    public async Task DownloadAsync(string filename, string destinationPath, DownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(filename, nameof(filename));
        Guard.NotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        var entry = await GetEntryAsync(filename, cancellationToken).ConfigureAwait(false);
        await _downloadService.DownloadAsync(entry, destinationPath, options ?? _options.Download, cancellationToken).ConfigureAwait(false);
    }

    public async Task DownloadToDirectoryAsync(string filename, string destinationDirectory, DownloadOptions? options = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(filename, nameof(filename));
        Guard.NotNullOrWhiteSpace(destinationDirectory, nameof(destinationDirectory));

        var entry = await GetEntryAsync(filename, cancellationToken).ConfigureAwait(false);
        var destinationPath = PathSafety.CombineAndEnsureSafe(destinationDirectory, entry.Filename);
        await _downloadService.DownloadAsync(entry, destinationPath, options ?? _options.Download, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ManifestEntry> GetEntryAsync(string filename, CancellationToken cancellationToken)
    {
        var manifest = await GetManifestAsync(cancellationToken).ConfigureAwait(false);
        var entry = manifest.FirstOrDefault(item => string.Equals(item.Filename, filename, StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            throw new FileNotFoundException($"Manifest entry not found for '{filename}'.", filename);
        }

        return entry;
    }
}
