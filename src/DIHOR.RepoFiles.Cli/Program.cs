using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles;
using DIHOR.RepoFiles.Cli.Commands;
using DIHOR.RepoFiles.Configuration;
using DIHOR.RepoFiles.GitHub;
using DIHOR.RepoFiles.GitHub.Providers;
using Microsoft.Extensions.Configuration;

namespace DIHOR.RepoFiles.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();
            return 1;
        }

        CliOptions options;
        try
        {
            options = ParseArgs(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            PrintUsage();
            return 1;
        }

        try
        {
            var configuration = BuildConfiguration(options.ConfigPath);
            var repoOptions = configuration.GetRepoFilesOptions();
            ValidateOptions(repoOptions);

            using var provider = CreateProvider(repoOptions);
            var client = new RepoClient(provider, new RepoFilesOptions
            {
                Download = repoOptions.Download.Clone()
            });

            var cancellationToken = CancellationToken.None;
            return options.Command switch
            {
                "list" => await ListCommand.ExecuteAsync(client, cancellationToken).ConfigureAwait(false),
                "pull" => await ExecutePullAsync(client, repoOptions, options, cancellationToken).ConfigureAwait(false),
                _ => 1
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static async Task<int> ExecutePullAsync(RepoClient client, RepoFilesConfigurationOptions repoOptions, CliOptions cliOptions, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cliOptions.Filename))
        {
            throw new InvalidOperationException("Filename is required for pull.");
        }

        var downloadOptions = repoOptions.Download.Clone();
        var destination = cliOptions.DestinationDirectory
            ?? downloadOptions.TargetDirectory
            ?? Directory.GetCurrentDirectory();

        downloadOptions.TargetDirectory = destination;
        return await PullCommand.ExecuteAsync(client, cliOptions.Filename, destination, downloadOptions, cancellationToken).ConfigureAwait(false);
    }

    private static IConfigurationRoot BuildConfiguration(string? configPath)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
        {
            builder.AddJsonFile(configPath, optional: false);
        }
        else if (File.Exists("appsettings.json"))
        {
            builder.AddJsonFile("appsettings.json", optional: false);
        }

        builder.AddEnvironmentVariables();
        return builder.Build();
    }

    private static void ValidateOptions(RepoFilesConfigurationOptions options)
    {
        if (!string.Equals(options.Provider, "GitHub", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("RepoFiles:Provider must be 'GitHub'.");
        }

        if (string.IsNullOrWhiteSpace(options.GitHub.Owner))
        {
            throw new InvalidOperationException("RepoFiles:GitHub:Owner is required.");
        }

        if (string.IsNullOrWhiteSpace(options.GitHub.Repository))
        {
            throw new InvalidOperationException("RepoFiles:GitHub:Repository is required.");
        }

        if (string.IsNullOrWhiteSpace(options.GitHub.Branch))
        {
            throw new InvalidOperationException("RepoFiles:GitHub:Branch is required.");
        }

        if (string.IsNullOrWhiteSpace(options.GitHub.ManifestPath))
        {
            throw new InvalidOperationException("RepoFiles:GitHub:ManifestPath is required.");
        }
    }

    private static GitHubRepositoryProvider CreateProvider(RepoFilesConfigurationOptions options)
    {
        var gitHubOptions = new GitHubRepositoryOptions
        {
            Owner = options.GitHub.Owner,
            Repository = options.GitHub.Repository,
            Branch = options.GitHub.Branch,
            ManifestPath = options.GitHub.ManifestPath
        };

        return new GitHubRepositoryProvider(gitHubOptions);
    }

    private static CliOptions ParseArgs(string[] args)
    {
        var command = args[0].ToLowerInvariant();
        if (command != "list" && command != "pull")
        {
            throw new InvalidOperationException("Unknown command.");
        }

        var options = new CliOptions { Command = command };
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--config":
                case "-c":
                    if (i + 1 >= args.Length)
                    {
                        throw new InvalidOperationException("Missing value for --config.");
                    }

                    options.ConfigPath = args[++i];
                    break;
                case "--dest":
                case "-d":
                    if (i + 1 >= args.Length)
                    {
                        throw new InvalidOperationException("Missing value for --dest.");
                    }

                    options.DestinationDirectory = args[++i];
                    break;
                default:
                    if (command == "pull" && string.IsNullOrWhiteSpace(options.Filename))
                    {
                        options.Filename = arg;
                        break;
                    }

                    throw new InvalidOperationException($"Unknown argument '{arg}'.");
            }
        }

        return options;
    }

    private static bool IsHelp(string arg)
    {
        return string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dihor-repofiles list [--config <path>]");
        Console.WriteLine("  dihor-repofiles pull <filename> [--dest <dir>] [--config <path>]");
    }

    private sealed class CliOptions
    {
        public string Command { get; set; } = string.Empty;

        public string? ConfigPath { get; set; }

        public string? Filename { get; set; }

        public string? DestinationDirectory { get; set; }
    }
}
