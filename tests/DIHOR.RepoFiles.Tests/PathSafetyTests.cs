using System;
using System.IO;
using DIHOR.RepoFiles.Utils;
using Xunit;

namespace DIHOR.RepoFiles.Tests;

public sealed class PathSafetyTests
{
    [Fact]
    public void CombinesSafePathWithinBaseDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var combined = PathSafety.CombineAndEnsureSafe(baseDir, "subdir/file.txt");

        Assert.StartsWith(Path.GetFullPath(baseDir), combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThrowsWhenPathEscapesBaseDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Throws<InvalidOperationException>(() => PathSafety.CombineAndEnsureSafe(baseDir, "../evil.txt"));
    }
}
