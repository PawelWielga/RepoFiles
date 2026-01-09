using System.Threading;
using System.Threading.Tasks;

namespace DIHOR.RepoFiles.Abstractions;

public interface IRepositoryPublisher
{
    Task PublishAsync(string sourcePath, string destinationPath, string? note = null, CancellationToken cancellationToken = default);
}
