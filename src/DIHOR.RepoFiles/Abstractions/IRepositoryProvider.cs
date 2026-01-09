using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Models;

namespace DIHOR.RepoFiles.Abstractions;

public interface IRepositoryProvider
{
    Task<Stream> OpenManifestStreamAsync(CancellationToken cancellationToken = default);

    Task<Stream> OpenFileStreamAsync(ManifestEntry entry, CancellationToken cancellationToken = default);
}
