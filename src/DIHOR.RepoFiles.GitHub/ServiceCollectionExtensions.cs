using System;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.GitHub.Publishers;
using DIHOR.RepoFiles.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DIHOR.RepoFiles.GitHub;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepoFilesGitHubPublisher(this IServiceCollection services, GitHubPublisherOptions options)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(options, nameof(options));

        services.AddSingleton(options);
        services.AddSingleton<IRepositoryPublisher, GitHubRepositoryPublisher>();
        return services;
    }

    public static IServiceCollection AddRepoFilesGitHubPublisher(this IServiceCollection services, Action<GitHubPublisherOptions> configure)
    {
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(configure, nameof(configure));

        var options = new GitHubPublisherOptions();
        configure(options);
        return services.AddRepoFilesGitHubPublisher(options);
    }
}
