using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles.Services;

public sealed class DownloadService
{
    private readonly IRepositoryProvider _provider;
    private readonly UpdateDecisionService _decisionService;

    public DownloadService(IRepositoryProvider provider, UpdateDecisionService decisionService)
    {
        Guard.NotNull(provider, nameof(provider));
        Guard.NotNull(decisionService, nameof(decisionService));
        _provider = provider;
        _decisionService = decisionService;
    }

    public async Task DownloadAsync(ManifestEntry entry, string destinationPath, DownloadOptions options, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entry, nameof(entry));
        Guard.NotNullOrWhiteSpace(destinationPath, nameof(destinationPath));
        Guard.NotNull(options, nameof(options));

        var destinationInfo = new FileInfo(destinationPath);
        if (!_decisionService.ShouldDownload(entry, destinationInfo, options))
        {
            return;
        }

        if (options.EnsureDirectoryExists)
        {
            var directory = destinationInfo.Directory?.FullName ?? Directory.GetCurrentDirectory();
            Directory.CreateDirectory(directory);
        }

        using var source = await _provider.OpenFileStreamAsync(entry, cancellationToken).ConfigureAwait(false);
        await StreamCopy.CopyToFileAsync(source, destinationPath, options.Overwrite, cancellationToken).ConfigureAwait(false);

        if (options.SetLastWriteTimeFromManifest && entry.ModifyDate != DateTimeOffset.MinValue)
        {
            File.SetLastWriteTimeUtc(destinationPath, entry.ModifyDate.UtcDateTime);
        }
    }
}
