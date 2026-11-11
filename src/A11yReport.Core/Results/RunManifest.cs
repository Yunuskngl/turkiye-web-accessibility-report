using System.Text.Json;

namespace A11yReport.Core.Results;

/// <summary>
/// The audit record of one scan run, exported as JSON next to the data: what was scanned,
/// when, with which tool versions and settings, and how many requests were made.
/// </summary>
public sealed record RunManifest(
    string Name,
    string State,
    string ListSource,
    string ListSha256,
    IReadOnlyList<RunSessionInfo> Sessions,
    IReadOnlyDictionary<string, int> StatusCounts,
    RequestCounts RequestTotals,
    IReadOnlyList<ManifestSite> Sites);

public sealed record RunSessionInfo(
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string State,
    ToolVersions Versions,
    string UserAgent,
    JsonElement Settings);

public sealed record ToolVersions(string Scanner, string Dotnet, string Playwright, string Browser, string? AxeCore);

public sealed record ManifestSite(
    string Domain,
    string Sector,
    string Status,
    int Attempts,
    int? HttpStatus,
    RequestCounts Requests,
    string? AxeVersion);
