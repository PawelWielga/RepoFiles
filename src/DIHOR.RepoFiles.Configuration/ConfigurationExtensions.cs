using DIHOR.RepoFiles.Utils;
using Microsoft.Extensions.Configuration;

namespace DIHOR.RepoFiles.Configuration;

public static class ConfigurationExtensions
{
    public static RepoFilesConfigurationOptions GetRepoFilesOptions(this IConfiguration configuration, string sectionName = "RepoFiles")
    {
        Guard.NotNull(configuration, nameof(configuration));
        Guard.NotNullOrWhiteSpace(sectionName, nameof(sectionName));

        var section = configuration.GetSection(sectionName);
        var options = new RepoFilesConfigurationOptions
        {
            Provider = section["Provider"] ?? string.Empty
        };

        var gitHubSection = section.GetSection("GitHub");
        options.GitHub = new GitHubOptions
        {
            Owner = gitHubSection["Owner"] ?? string.Empty,
            Repository = gitHubSection["Repository"] ?? string.Empty,
            Branch = gitHubSection["Branch"] ?? "main",
            ManifestPath = gitHubSection["ManifestPath"] ?? "manifest.json"
        };

        var gitHubPublisherSection = section.GetSection("GitHubPublisher");
        options.GitHubPublisher = new GitHubPublisherOptions
        {
            Owner = gitHubPublisherSection["Owner"] ?? string.Empty,
            Repository = gitHubPublisherSection["Repository"] ?? string.Empty,
            Branch = gitHubPublisherSection["Branch"] ?? "main",
            Token = gitHubPublisherSection["Token"] ?? string.Empty,
            CommitterName = gitHubPublisherSection["CommitterName"],
            CommitterEmail = gitHubPublisherSection["CommitterEmail"]
        };

        var downloadSection = section.GetSection("Download");
        options.Download = new DownloadOptions
        {
            TargetDirectory = downloadSection["TargetDirectory"],
            SkipIfSameSizeAndDate = GetBoolean(downloadSection, "SkipIfSameSizeAndDate", true),
            Overwrite = GetBoolean(downloadSection, "Overwrite", false),
            EnsureDirectoryExists = GetBoolean(downloadSection, "EnsureDirectoryExists", true),
            SetLastWriteTimeFromManifest = GetBoolean(downloadSection, "SetLastWriteTimeFromManifest", true)
        };

        return options;
    }

    private static bool GetBoolean(IConfiguration section, string key, bool defaultValue)
    {
        var value = section[key];
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
