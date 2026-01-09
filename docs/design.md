# Design notes

- Core library is provider-agnostic and targets netstandard2.0.
- GitHub provider builds raw GitHub URLs and streams content via HttpClient.
- Configuration package binds IConfiguration to RepoFiles options without GitHub dependency.
- CLI reads appsettings.json and uses the GitHub provider for list/pull.
