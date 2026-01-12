using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.GitHub;
using DIHOR.RepoFiles.GitHub.Publishers;

namespace DIHOR.RepoFiles.Tests;

public sealed class GitHubRepositoryPublisherTests
{
    [Fact]
    public async Task PublishAsync_CreatesFile_WhenMissing()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "payload");
            var handler = new SequenceHandler(new[]
            {
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{\"message\":\"Not Found\"}")
                },
                new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("{\"content\":{}}")
                }
            });

            var options = new GitHubPublisherOptions
            {
                Owner = "owner",
                Repository = "repo",
                Branch = "main",
                Token = "token"
            };

            var publisher = new GitHubRepositoryPublisher(options, new HttpClient(handler));
            await publisher.PublishAsync(tempPath, "Distribution/manifest.json");

            Assert.Equal(2, handler.Requests.Count);
            Assert.Equal(HttpMethod.Get, handler.Requests[0].Method);
            Assert.Contains("ref=main", handler.Requests[0].Uri.Query);

            var putRequest = handler.Requests[1];
            Assert.Equal(HttpMethod.Put, putRequest.Method);
            Assert.Contains("/repos/owner/repo/contents/Distribution/manifest.json", putRequest.Uri.ToString());

            using var payload = JsonDocument.Parse(putRequest.Body);
            Assert.False(payload.RootElement.TryGetProperty("sha", out _));
            Assert.Equal("main", payload.RootElement.GetProperty("branch").GetString());
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task PublishAsync_OverwritesFile_WhenShaAvailable()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "payload");
            var handler = new SequenceHandler(new[]
            {
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"sha\":\"abc123\"}")
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"content\":{}}")
                }
            });

            var options = new GitHubPublisherOptions
            {
                Owner = "owner",
                Repository = "repo",
                Branch = "main",
                Token = "token"
            };

            var publisher = new GitHubRepositoryPublisher(options, new HttpClient(handler));
            await publisher.PublishAsync(tempPath, "sqlite.db", "Update file");

            var putRequest = handler.Requests[1];
            using var payload = JsonDocument.Parse(putRequest.Body);
            Assert.Equal("abc123", payload.RootElement.GetProperty("sha").GetString());
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task PublishAsync_Throws_WhenTokenMissing()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "payload");
            var handler = new SequenceHandler(Array.Empty<HttpResponseMessage>());
            var options = new GitHubPublisherOptions
            {
                Owner = "owner",
                Repository = "repo",
                Branch = "main",
                Token = ""
            };

            var publisher = new GitHubRepositoryPublisher(options, new HttpClient(handler));
            await Assert.ThrowsAsync<InvalidOperationException>(() => publisher.PublishAsync(tempPath, "sqlite.db"));
            Assert.Empty(handler.Requests);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public SequenceHandler(IEnumerable<HttpResponseMessage> responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<CapturedRequest> Requests { get; } = new List<CapturedRequest>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri ?? new Uri("about:blank"), body));

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("No response configured for request.");
            }

            return _responses.Dequeue();
        }
    }

    private sealed class CapturedRequest
    {
        public CapturedRequest(HttpMethod method, Uri uri, string body)
        {
            Method = method;
            Uri = uri;
            Body = body;
        }

        public HttpMethod Method { get; }

        public Uri Uri { get; }

        public string Body { get; }
    }
}
