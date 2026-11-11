namespace A11yReport.Storage;

public enum RunState
{
    Running,
    Completed,
    Interrupted,
}

public sealed class ScanRunEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ListSource { get; set; }
    public required string ListSha256 { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public RunState State { get; set; }
    public List<RunSessionEntity> Sessions { get; set; } = [];
    public List<SiteScanEntity> Sites { get; set; } = [];
}

/// <summary>One start or resume of a run, with the exact environment it ran in.</summary>
public sealed class RunSessionEntity
{
    public int Id { get; set; }
    public int RunId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public RunState State { get; set; }
    public required string ScannerVersion { get; set; }
    public required string DotnetVersion { get; set; }
    public required string PlaywrightVersion { get; set; }
    public required string BrowserVersion { get; set; }
    public string? AxeVersion { get; set; }
    public required string UserAgent { get; set; }
    public required string SettingsJson { get; set; }
}

public sealed class SiteScanEntity
{
    public int Id { get; set; }
    public int RunId { get; set; }
    public int SessionId { get; set; }
    public required string Domain { get; set; }
    public required string Sector { get; set; }
    public required string Url { get; set; }
    public A11yReport.Core.Results.ScanStatus Status { get; set; }
    public required string Message { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset FinishedAt { get; set; }
    public int? HttpStatus { get; set; }
    public string? FinalUrl { get; set; }
    public string? FinalSiteKey { get; set; }
    public string? AxeVersion { get; set; }

    /// <summary>False when we scanned after DOMContentLoaded without the full load event.</summary>
    public bool? LoadEventReached { get; set; }

    public int RobotsRequests { get; set; }
    public int DocumentRequests { get; set; }
    public int SubresourceRequests { get; set; }

    public string? Lang { get; set; }
    public bool? HasTitle { get; set; }
    public int? LandmarkCount { get; set; }
    public bool? HasMain { get; set; }
    public int? HeadingCount { get; set; }
    public int? H1Count { get; set; }
    public bool? SkipsHeadingLevel { get; set; }

    public List<ViolationEntity> Violations { get; set; } = [];
}

public sealed class ViolationEntity
{
    public int Id { get; set; }
    public int SiteScanId { get; set; }
    public required string Viewport { get; set; }
    public required string RuleId { get; set; }
    public required string Impact { get; set; }
    public required string Help { get; set; }
    public required string Tags { get; set; }
    public int NodeCount { get; set; }
    public List<ViolationNodeEntity> Nodes { get; set; } = [];
}

public sealed class ViolationNodeEntity
{
    public int Id { get; set; }
    public int ViolationId { get; set; }
    public required string Selector { get; set; }
    public required string Snippet { get; set; }
}
