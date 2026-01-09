using System;
using System.IO;
using System.Threading.Tasks;
using DIHOR.RepoFiles;
using DIHOR.RepoFiles.Configuration;
using DIHOR.RepoFiles.GitHub;
using DIHOR.RepoFiles.GitHub.Providers;
using Microsoft.Extensions.Configuration;

namespace Sample.ConsoleApp;

internal static class Program
{
    private static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var repoOptions = configuration.GetRepoFilesOptions();
        if (!string.Equals(repoOptions.Provider, "GitHub", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("RepoFiles:Provider must be 'GitHub'.");
            return;
        }

        using var provider = new GitHubRepositoryProvider(new GitHubRepositoryOptions
        {
            Owner = repoOptions.GitHub.Owner,
            Repository = repoOptions.GitHub.Repository,
            Branch = repoOptions.GitHub.Branch,
            ManifestPath = repoOptions.GitHub.ManifestPath
        });

        var client = new RepoClient(provider, new RepoFilesOptions
        {
            Download = repoOptions.Download.Clone()
        });

        var targetDirectory = repoOptions.Download.TargetDirectory ?? "Data";
        await client.DownloadToDirectoryAsync("nmn.db", targetDirectory);
        Console.WriteLine("Download complete.");
    }
}
