# DIHOR RepoFiles

DIHOR RepoFiles is a small set of NuGet packages for managing files stored in a repository using a JSON manifest.

## Packages

- `DIHOR.RepoFiles` (core)
- `DIHOR.RepoFiles.GitHub` (GitHub provider)
- `DIHOR.RepoFiles.Configuration` (IConfiguration binding)
- `DIHOR.RepoFiles.Cli` (CLI tool)

## Install

```bash
dotnet add package DIHOR.RepoFiles
dotnet add package DIHOR.RepoFiles.GitHub
dotnet add package DIHOR.RepoFiles.Configuration
```

```bash
dotnet tool install -g DIHOR.RepoFiles.Cli
```

## Manifest format

`manifest.json` is a JSON array of entries:

```json
[
  {
    "filename": "data.db",
    "url": "https://raw.githubusercontent.com/owner/repo/main/data.db",
    "size": 123456,
    "modifydate": "2024-01-01T12:00:00Z",
    "note": "Optional note"
  }
]
```

## Configuration (appsettings.json)

```json
{
  "RepoFiles": {
    "Provider": "GitHub",
    "GitHub": {
      "Owner": "your-org",
      "Repository": "your-repo",
      "Branch": "main",
      "ManifestPath": "manifest.json"
    },
    "Download": {
      "TargetDirectory": "Data",
      "SkipIfSameSizeAndDate": true,
      "Overwrite": false
    }
  }
}
```

## Usage (code)

```csharp
using DIHOR.RepoFiles;
using DIHOR.RepoFiles.GitHub;
using DIHOR.RepoFiles.GitHub.Providers;

var provider = new GitHubRepositoryProvider(new GitHubRepositoryOptions
{
    Owner = "your-org",
    Repository = "your-repo",
    Branch = "main",
    ManifestPath = "manifest.json"
});

var client = new RepoClient(provider);
await client.DownloadToDirectoryAsync("data.db", "Data");
```

## CLI

```bash
dihor-repofiles list
dihor-repofiles pull data.db --dest Data
```

The CLI reads `appsettings.json` from the current directory (or pass `--config`).

## Publish to NuGet

```bash
dotnet pack -c Release
```

Push the packages to NuGet.org (repeat for each project output folder):

```bash
dotnet nuget push "src/DIHOR.RepoFiles/bin/Release/*.nupkg" -k <NUGET_API_KEY> -s https://api.nuget.org/v3/index.json --skip-duplicate
dotnet nuget push "src/DIHOR.RepoFiles.GitHub/bin/Release/*.nupkg" -k <NUGET_API_KEY> -s https://api.nuget.org/v3/index.json --skip-duplicate
dotnet nuget push "src/DIHOR.RepoFiles.Configuration/bin/Release/*.nupkg" -k <NUGET_API_KEY> -s https://api.nuget.org/v3/index.json --skip-duplicate
dotnet nuget push "src/DIHOR.RepoFiles.Cli/bin/Release/*.nupkg" -k <NUGET_API_KEY> -s https://api.nuget.org/v3/index.json --skip-duplicate
```

Tip: store the key in an environment variable (`NUGET_API_KEY`) or use `dotnet nuget setapikey`.
