using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Services;
using Xunit;

namespace DIHOR.RepoFiles.Tests;

public sealed class ManifestServiceTests
{
    [Fact]
    public async Task ParsesCaseInsensitivePropertiesAndDates()
    {
        var json = "[" +
                   "{\"FileName\":\"a.txt\",\"URL\":\"https://example.com/a.txt\",\"SIZE\":10,\"ModifyDate\":\"2024-01-01T00:00:00Z\",\"Metadata\":\"{\\\"minAppVersion\\\":\\\"1.2.3\\\"}\"}," +
                   "{\"filename\":\"b.txt\",\"size\":\"20\",\"modifydate\":\"2024-01-02 12:34:56\"}" +
                   "]";

        var provider = new InMemoryRepositoryProvider(json);
        var service = new ManifestService(provider);

        var entries = await service.GetManifestAsync();

        Assert.Equal(2, entries.Count);
        Assert.Equal("a.txt", entries[0].Filename);
        Assert.Equal(10, entries[0].Size);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), entries[0].ModifyDate);
        Assert.Equal("{\"minAppVersion\":\"1.2.3\"}", entries[0].MetadataJson);
        Assert.Equal("b.txt", entries[1].Filename);
        Assert.Equal(20, entries[1].Size);
        Assert.Equal(new DateTimeOffset(2024, 1, 2, 12, 34, 56, TimeSpan.Zero), entries[1].ModifyDate);
        Assert.Null(entries[1].MetadataJson);
    }

    [Fact]
    public async Task ParsesMetadataIntoTypedEntries()
    {
        var json = "[" +
                   "{\"filename\":\"a.txt\",\"size\":10,\"metadata\":\"{\\\"minAppVersion\\\":\\\"2.0\\\",\\\"isRequired\\\":true}\"}" +
                   "]";

        var provider = new InMemoryRepositoryProvider(json);
        var service = new ManifestService(provider);

        var entries = await service.GetManifestAsync<SampleMetadata>();

        Assert.Single(entries);
        Assert.Equal("a.txt", entries[0].Filename);
        Assert.NotNull(entries[0].Metadata);
        Assert.Equal("2.0", entries[0].Metadata!.MinAppVersion);
        Assert.True(entries[0].Metadata!.IsRequired);
    }

    [Fact]
    public async Task ThrowsWhenManifestRootIsNotArray()
    {
        var json = "{ \"filename\": \"a.txt\" }";
        var provider = new InMemoryRepositoryProvider(json);
        var service = new ManifestService(provider);

        await Assert.ThrowsAsync<FormatException>(() => service.GetManifestAsync());
    }

    [Fact]
    public async Task ThrowsWhenRequiredPropertyMissing()
    {
        var json = "[{ \"size\": 10 }]";
        var provider = new InMemoryRepositoryProvider(json);
        var service = new ManifestService(provider);

        await Assert.ThrowsAsync<FormatException>(() => service.GetManifestAsync());
    }

    private sealed class InMemoryRepositoryProvider : IRepositoryProvider
    {
        private readonly string _manifestJson;

        public InMemoryRepositoryProvider(string manifestJson)
        {
            _manifestJson = manifestJson;
        }

        public Task<Stream> OpenManifestStreamAsync(CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(_manifestJson);
            return Task.FromResult<Stream>(new MemoryStream(bytes));
        }

        public Task<Stream> OpenFileStreamAsync(ManifestEntry entry, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }
    }

    private sealed class SampleMetadata
    {
        public string? MinAppVersion { get; set; }

        public bool IsRequired { get; set; }
    }
}
