using System;
using System.IO;

namespace DIHOR.RepoFiles.Utils;

public static class PathSafety
{
    public static string CombineAndEnsureSafe(string baseDirectory, string relativePath)
    {
        Guard.NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));
        Guard.NotNullOrWhiteSpace(relativePath, nameof(relativePath));

        var baseFull = Path.GetFullPath(baseDirectory);
        baseFull = baseFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var combinedFull = Path.GetFullPath(Path.Combine(baseFull, relativePath));

        if (!combinedFull.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes base directory.");
        }

        return combinedFull;
    }
}
