using System;

namespace DIHOR.RepoFiles.Models;

public sealed class ManifestEntry
{
    public string Filename { get; set; } = string.Empty;

    public string? Url { get; set; }

    public long Size { get; set; }

    public DateTimeOffset ModifyDate { get; set; }

    public string? Note { get; set; }
}
