using System;
using System.IO;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Services;
using Xunit;

namespace DIHOR.RepoFiles.Tests;

public sealed class UpdateDecisionServiceTests
{
    [Fact]
    public void DownloadsWhenFileIsMissing()
    {
        var entry = new ManifestEntry
        {
            Filename = "file.txt",
            Size = 5,
            ModifyDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var tempDir = CreateTempDirectory();
        var fileInfo = new FileInfo(Path.Combine(tempDir, "file.txt"));

        var service = new UpdateDecisionService();
        var options = new DownloadOptions { Overwrite = false, SkipIfSameSizeAndDate = true };

        Assert.True(service.ShouldDownload(entry, fileInfo, options));
    }

    [Fact]
    public void SkipsWhenSameSizeAndDate()
    {
        var tempDir = CreateTempDirectory();
        var filePath = Path.Combine(tempDir, "file.txt");
        File.WriteAllBytes(filePath, new byte[4]);
        File.SetLastWriteTimeUtc(filePath, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var entry = new ManifestEntry
        {
            Filename = "file.txt",
            Size = 4,
            ModifyDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var service = new UpdateDecisionService();
        var options = new DownloadOptions { Overwrite = false, SkipIfSameSizeAndDate = true };

        Assert.False(service.ShouldDownload(entry, new FileInfo(filePath), options));
    }

    [Fact]
    public void DownloadsWhenDifferentAndOverwriteEnabled()
    {
        var tempDir = CreateTempDirectory();
        var filePath = Path.Combine(tempDir, "file.txt");
        File.WriteAllBytes(filePath, new byte[4]);
        File.SetLastWriteTimeUtc(filePath, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var entry = new ManifestEntry
        {
            Filename = "file.txt",
            Size = 8,
            ModifyDate = new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)
        };

        var service = new UpdateDecisionService();
        var options = new DownloadOptions { Overwrite = true, SkipIfSameSizeAndDate = true };

        Assert.True(service.ShouldDownload(entry, new FileInfo(filePath), options));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
