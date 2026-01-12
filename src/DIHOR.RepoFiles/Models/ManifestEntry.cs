using System;

namespace DIHOR.RepoFiles.Models;

public class ManifestEntry
{
    public string Filename { get; set; } = string.Empty;

    public string? Url { get; set; }

    public long Size { get; set; }

    public DateTimeOffset ModifyDate { get; set; }

    public string? MetadataJson { get; set; }
}
