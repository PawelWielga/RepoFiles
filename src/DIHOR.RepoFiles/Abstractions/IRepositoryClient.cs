using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Models;

namespace DIHOR.RepoFiles.Abstractions;

public interface IRepositoryClient
{
    Task<IReadOnlyList<ManifestEntry>> GetManifestAsync(CancellationToken cancellationToken = default);

    Task DownloadAsync(string filename, string destinationPath, DownloadOptions? options = null, CancellationToken cancellationToken = default);

    Task DownloadToDirectoryAsync(string filename, string destinationDirectory, DownloadOptions? options = null, CancellationToken cancellationToken = default);
}
