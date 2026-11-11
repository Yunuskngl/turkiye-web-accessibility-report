using A11yReport.Core.Results;

namespace A11yReport.Core.Report;

/// <summary>Number of failing elements for one axe rule on one page and viewport.</summary>
public sealed record RuleCount(string RuleId, string Impact, string Help, int Elements)
{
    /// <summary>WCAG success criteria of the rule, for example "1.4.3".</summary>
    public IReadOnlyList<string> Criteria { get; init; } = [];
}

/// <summary>Everything the report needs about one site in one run. No selectors or snippets.</summary>
public sealed record SiteOutcome(
    string Domain,
    string Sector,
    ScanStatus Status,
    PageFacts? Facts,
    IReadOnlyList<RuleCount> Desktop,
    IReadOnlyList<RuleCount> Mobile,
    bool? LoadEventReached);

/// <summary>Facts about the run shown on the report and the methodology page.</summary>
public sealed record RunInfo(
    string Name,
    string State,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string AxeVersion,
    string BrowserVersion,
    string ListSource);
