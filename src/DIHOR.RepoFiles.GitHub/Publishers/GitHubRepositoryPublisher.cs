using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.GitHub.Http;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles.GitHub.Publishers;

public sealed class GitHubRepositoryPublisher : IRepositoryPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly GitHubPublisherOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeClient;

    public GitHubRepositoryPublisher(GitHubPublisherOptions options)
        : this(options, null)
    {
    }

    public GitHubRepositoryPublisher(GitHubPublisherOptions options, HttpClient? httpClient)
    {
        Guard.NotNull(options, nameof(options));
        Guard.NotNullOrWhiteSpace(options.Owner, nameof(options.Owner));
        Guard.NotNullOrWhiteSpace(options.Repository, nameof(options.Repository));

        _options = options;
        if (string.IsNullOrWhiteSpace(_options.Branch))
        {
            _options.Branch = "main";
        }

        _httpClient = httpClient ?? new HttpClient();
        _disposeClient = httpClient is null;

        ConfigureHttpClient(_httpClient);
    }

    public async Task PublishAsync(string sourcePath, string destinationPath, string? note = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        Guard.NotNullOrWhiteSpace(destinationPath, nameof(destinationPath));

        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            throw new InvalidOperationException("GitHub token is required to publish files.");
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source file not found '{sourcePath}'.", sourcePath);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var normalizedPath = NormalizePath(destinationPath);
        var contentBytes = File.ReadAllBytes(sourcePath);
        cancellationToken.ThrowIfCancellationRequested();

        var commitMessage = string.IsNullOrWhiteSpace(note)
            ? $"Publish {normalizedPath}"
            : note!;

        var sha = await TryGetShaAsync(normalizedPath, cancellationToken).ConfigureAwait(false);

        var payload = new GitHubContentRequest
        {
            Message = commitMessage,
            Content = Convert.ToBase64String(contentBytes),
            Branch = _options.Branch,
            Sha = sha,
            Committer = BuildCommitter()
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Put, GitHubApiUrlBuilder.BuildContentsUri(_options.Owner, _options.Repository, normalizedPath))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = BuildAuthHeader(_options.Token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiException(response, "publish").ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient.Dispose();
        }
    }

    private async Task<string?> TryGetShaAsync(string destinationPath, CancellationToken cancellationToken)
    {
        var uri = GitHubApiUrlBuilder.BuildContentsUri(_options.Owner, _options.Repository, destinationPath, _options.Branch);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = BuildAuthHeader(_options.Token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiException(response, "fetch").ConfigureAwait(false);
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var payload = JsonSerializer.Deserialize<GitHubContentResponse>(json, JsonOptions);
        var remoteSha = payload?.Sha;
        if (string.IsNullOrWhiteSpace(remoteSha))
        {
            throw new InvalidOperationException("GitHub response did not include file SHA.");
        }

        return remoteSha;
    }

    private GitHubCommitter? BuildCommitter()
    {
        if (string.IsNullOrWhiteSpace(_options.CommitterName)
            && string.IsNullOrWhiteSpace(_options.CommitterEmail))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_options.CommitterName)
            || string.IsNullOrWhiteSpace(_options.CommitterEmail))
        {
            throw new InvalidOperationException("Both CommitterName and CommitterEmail must be set when specifying a committer.");
        }

        var committerName = _options.CommitterName!;
        var committerEmail = _options.CommitterEmail!;
        return new GitHubCommitter
        {
            Name = committerName,
            Email = committerEmail
        };
    }

    private static AuthenticationHeaderValue BuildAuthHeader(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthenticationHeaderValue("Bearer", trimmed.Substring("Bearer ".Length).Trim());
        }

        if (trimmed.StartsWith("token ", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthenticationHeaderValue("token", trimmed.Substring("token ".Length).Trim());
        }

        return new AuthenticationHeaderValue("Bearer", trimmed);
    }

    private static void ConfigureHttpClient(HttpClient httpClient)
    {
        if (!httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DIHOR.RepoFiles.GitHub");
        }

        if (!httpClient.DefaultRequestHeaders.Accept.Any(header =>
                string.Equals(header.MediaType, "application/vnd.github+json", StringComparison.OrdinalIgnoreCase)))
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }

    private static async Task<Exception> CreateApiException(HttpResponseMessage response, string action)
    {
        var body = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var details = TryReadApiMessage(body);
        var statusText = $"{(int)response.StatusCode} {response.ReasonPhrase}".Trim();
        var message = $"GitHub API {action} failed ({statusText}).";

        if (!string.IsNullOrWhiteSpace(details))
        {
            message = $"{message} {details}";
        }

        if (response.StatusCode == HttpStatusCode.Conflict
            || (int)response.StatusCode == 422)
        {
            message = $"{message} The remote file may have changed; fetch the latest SHA and retry.";
        }

        return new GitHubApiException(response.StatusCode, message, body);
    }

    private static string? TryReadApiMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var error = JsonSerializer.Deserialize<GitHubApiError>(body, JsonOptions);
            if (error is null || string.IsNullOrWhiteSpace(error.Message))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(error.DocumentationUrl))
            {
                return error.Message;
            }

            return $"{error.Message} ({error.DocumentationUrl})";
        }
        catch (JsonException)
        {
            return body.Trim();
        }
    }

    private sealed class GitHubContentRequest
    {
        public string Message { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string? Sha { get; set; }

        public GitHubCommitter? Committer { get; set; }
    }

    private sealed class GitHubCommitter
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }

    private sealed class GitHubContentResponse
    {
        public string? Sha { get; set; }
    }

    private sealed class GitHubApiError
    {
        public string? Message { get; set; }

        [JsonPropertyName("documentation_url")]
        public string? DocumentationUrl { get; set; }
    }
}
