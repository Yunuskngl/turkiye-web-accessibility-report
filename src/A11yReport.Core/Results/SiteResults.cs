namespace A11yReport.Core.Results;

public sealed record RequestCounts(int Robots, int Documents, int Subresources)
{
    public static RequestCounts None { get; } = new(0, 0, 0);

    public int Total => Robots + Documents + Subresources;

    public RequestCounts Add(RequestCounts other) =>
        new(Robots + other.Robots, Documents + other.Documents, Subresources + other.Subresources);
}

/// <summary>Structural facts about the page. Never page text: the title is stored as present or not.</summary>
public sealed record PageFacts(
    string? Lang,
    bool HasTitle,
    int LandmarkCount,
    bool HasMain,
    int HeadingCount,
    int H1Count,
    bool SkipsHeadingLevel);

public sealed record SiteScanResult(
    string Domain,
    string Sector,
    Uri Url,
    ScanStatus Status,
    string Message,
    int Attempts,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    RequestCounts Requests,
    int? HttpStatus = null,
    Uri? FinalUrl = null,
    string? AxeVersion = null,
    PageFacts? Facts = null,
    IReadOnlyList<ViewportResult>? Viewports = null,
    bool? LoadEventReached = null);
