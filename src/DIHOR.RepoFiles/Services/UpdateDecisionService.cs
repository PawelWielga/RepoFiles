using System;
using System.IO;
using DIHOR.RepoFiles.Models;

namespace DIHOR.RepoFiles.Services;

public sealed class UpdateDecisionService
{
    private static readonly TimeSpan TimeTolerance = TimeSpan.FromSeconds(2);

    public bool ShouldDownload(ManifestEntry entry, FileInfo destinationFile, DownloadOptions options)
    {
        if (!destinationFile.Exists)
        {
            return true;
        }

        if (options.SkipIfSameSizeAndDate && MatchesSizeAndDate(entry, destinationFile))
        {
            return false;
        }

        return options.Overwrite;
    }

    private static bool MatchesSizeAndDate(ManifestEntry entry, FileInfo destinationFile)
    {
        if (entry.Size > 0 && destinationFile.Length != entry.Size)
        {
            return false;
        }

        if (entry.ModifyDate != DateTimeOffset.MinValue)
        {
            var manifestUtc = entry.ModifyDate.UtcDateTime;
            var fileUtc = destinationFile.LastWriteTimeUtc;
            if ((fileUtc - manifestUtc).Duration() > TimeTolerance)
            {
                return false;
            }
        }

        return true;
    }
}
