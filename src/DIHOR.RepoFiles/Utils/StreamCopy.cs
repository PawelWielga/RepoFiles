using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DIHOR.RepoFiles.Utils;

public static class StreamCopy
{
    public static async Task CopyToFileAsync(Stream source, string destinationPath, bool overwrite, CancellationToken cancellationToken)
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using var target = new FileStream(destinationPath, fileMode, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await source.CopyToAsync(target, 81920, cancellationToken).ConfigureAwait(false);
    }
}
