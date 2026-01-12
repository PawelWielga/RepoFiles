using System;
using System.Net;

namespace DIHOR.RepoFiles.GitHub;

public sealed class GitHubApiException : Exception
{
    public GitHubApiException(HttpStatusCode statusCode, string message, string? response = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Response = response;
    }

    public HttpStatusCode StatusCode { get; }

    public string? Response { get; }
}
