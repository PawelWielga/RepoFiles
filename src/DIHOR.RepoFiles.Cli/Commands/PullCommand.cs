using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles;

namespace DIHOR.RepoFiles.Cli.Commands;

internal static class PullCommand
{
    public static async Task<int> ExecuteAsync(RepoClient client, string filename, string destinationDirectory, DownloadOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename is required.", nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            destinationDirectory = Directory.GetCurrentDirectory();
        }

        await client.DownloadToDirectoryAsync(filename, destinationDirectory, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"Downloaded '{filename}' to '{destinationDirectory}'.");
        return 0;
    }
}
