namespace DIHOR.RepoFiles.Models;

public sealed class ManifestEntry<TMetadata> : ManifestEntry
{
    public TMetadata? Metadata { get; set; }
}
