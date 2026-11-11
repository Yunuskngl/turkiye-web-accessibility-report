namespace A11yReport.Core.Results;

public sealed record ViolationNode(string Selector, string Snippet);

public sealed record RuleViolation(
    string RuleId,
    string Impact,
    string Help,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ViolationNode> Nodes)
{
    public int NodeCount => Nodes.Count;
}

public sealed record ViewportResult(
    string Name,
    int Width,
    int Height,
    IReadOnlyList<RuleViolation> Violations)
{
    public int NodeCount => Violations.Sum(violation => violation.NodeCount);
}

public sealed record PageScanResult(
    Uri RequestedUrl,
    Uri FinalUrl,
    int HttpStatus,
    string AxeVersion,
    DateTimeOffset ScannedAt,
    IReadOnlyList<ViewportResult> Viewports,
    PageFacts Facts,
    bool LoadEventReached = true);
