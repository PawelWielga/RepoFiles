namespace DIHOR.RepoFiles;

public sealed class DownloadOptions
{
    public string? TargetDirectory { get; set; }

    public bool SkipIfSameSizeAndDate { get; set; } = true;

    public bool Overwrite { get; set; } = false;

    public bool EnsureDirectoryExists { get; set; } = true;

    public bool SetLastWriteTimeFromManifest { get; set; } = true;

    public DownloadOptions Clone()
    {
        return new DownloadOptions
        {
            TargetDirectory = TargetDirectory,
            SkipIfSameSizeAndDate = SkipIfSameSizeAndDate,
            Overwrite = Overwrite,
            EnsureDirectoryExists = EnsureDirectoryExists,
            SetLastWriteTimeFromManifest = SetLastWriteTimeFromManifest
        };
    }
}
