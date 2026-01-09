using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DIHOR.RepoFiles.Abstractions;
using DIHOR.RepoFiles.Models;
using DIHOR.RepoFiles.Utils;

namespace DIHOR.RepoFiles.Services;

public sealed class ManifestService
{
    private readonly IRepositoryProvider _provider;

    public ManifestService(IRepositoryProvider provider)
    {
        Guard.NotNull(provider, nameof(provider));
        _provider = provider;
    }

    public async Task<IReadOnlyList<ManifestEntry>> GetManifestAsync(CancellationToken cancellationToken = default)
    {
        using var stream = await _provider.OpenManifestStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
        var json = await reader.ReadToEndAsync().ConfigureAwait(false);
        return ParseManifest(json);
    }

    private static IReadOnlyList<ManifestEntry> ParseManifest(string json)
    {
        Guard.NotNullOrWhiteSpace(json, nameof(json));
        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("Manifest root must be a JSON array.");
        }

        var entries = new List<ManifestEntry>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var filename = ReadRequiredString(element, "filename");
            var url = ReadOptionalString(element, "url");
            var size = ReadInt64(element, "size");
            var modifyDate = ReadModifyDate(element, "modifydate");
            var note = ReadOptionalString(element, "note");

            entries.Add(new ManifestEntry
            {
                Filename = filename,
                Url = url,
                Size = size,
                ModifyDate = modifyDate,
                Note = note
            });
        }

        return entries;
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var value))
        {
            throw new FormatException($"Missing required property '{propertyName}'.");
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        throw new FormatException($"Property '{propertyName}' must be a string.");
    }

    private static string? ReadOptionalString(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        throw new FormatException($"Property '{propertyName}' must be a string.");
    }

    private static long ReadInt64(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var value))
        {
            throw new FormatException($"Missing required property '{propertyName}'.");
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
            return number;
        }

        throw new FormatException($"Property '{propertyName}' must be a number.");
    }

    private static DateTimeOffset ReadModifyDate(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var value))
        {
            return DateTimeOffset.MinValue;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return DateTimeOffset.MinValue;
            }

            if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result))
            {
                return result;
            }
        }

        throw new FormatException($"Property '{propertyName}' must be a valid date string.");
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
