using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles;

namespace DIHOR.RepoFiles.Cli.Commands;

internal static class ListCommand
{
    public static async Task<int> ExecuteAsync(RepoClient client, CancellationToken cancellationToken)
    {
        var entries = await client.GetManifestAsync(cancellationToken).ConfigureAwait(false);
        foreach (var entry in entries)
        {
            var date = entry.ModifyDate == DateTimeOffset.MinValue
                ? "-"
                : entry.ModifyDate.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture);
            var metadata = string.IsNullOrWhiteSpace(entry.MetadataJson) ? "-" : entry.MetadataJson;
            Console.WriteLine($"{entry.Filename}\t{entry.Size}\t{date}\t{metadata}");
        }

        return 0;
    }
}
