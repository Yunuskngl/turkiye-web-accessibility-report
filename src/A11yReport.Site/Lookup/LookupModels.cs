namespace A11yReport.Site.Lookup;

/// <summary>One site's published result: rule ids, plain names and counts only. No selectors or snippets.</summary>
public sealed record LookupSite(
    string Domain,
    string SectorName,
    bool Checked,
    string StatusText,
    string CheckedOn,
    string AxeVersion,
    bool? LoadEventReached,
    LookupFacts? Facts,
    IReadOnlyList<LookupViewport> Viewports);

public sealed record LookupViewport(string Name, int FailedRules, int Elements, IReadOnlyList<LookupRule> Rules);

public sealed record LookupRule(string RuleId, string Name, string Criteria, int Elements);

public sealed record LookupFacts(string? Lang, bool HasTitle, bool HasMain, int H1Count, int HeadingCount, bool SkipsHeadingLevel);
