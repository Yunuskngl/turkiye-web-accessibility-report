using A11yReport.Core.Results;

namespace A11yReport.Tests.Unit;

public sealed class SummaryFormatterTests
{
    private static readonly Uri Url = new("https://example.com.tr/");

    [Fact]
    public void Build_NoViolations_SaysNothingDetected_NotAccessible()
    {
        var summary = SummaryFormatter.Build(Result(Viewport([])));

        Assert.Contains("No failures detected by the automated rules.", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("accessible", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not a compliance verdict", summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_SortsBySeverityThenElementCount()
    {
        var summary = SummaryFormatter.Build(Result(Viewport(
        [
            Violation("region", Impact.Moderate, nodes: 9),
            Violation("image-alt", Impact.Critical, nodes: 1),
            Violation("color-contrast", Impact.Serious, nodes: 2),
            Violation("link-name", Impact.Serious, nodes: 5),
        ])));

        string[] expectedOrder = ["image-alt", "link-name", "color-contrast", "region"];
        var positions = expectedOrder.Select(id => summary.IndexOf(id, StringComparison.Ordinal)).ToList();
        Assert.Equal(positions.Order(), positions);
    }

    [Fact]
    public void Build_ShowsCountsAndWcagCriteria()
    {
        var summary = SummaryFormatter.Build(Result(Viewport([Violation("color-contrast", Impact.Serious, nodes: 3)])));

        Assert.Contains("1 failed rules, 3 elements", summary, StringComparison.Ordinal);
        Assert.Contains("1.4.3", summary, StringComparison.Ordinal);
    }

    private static PageScanResult Result(ViewportResult viewport) =>
        new(Url, Url, 200, "4.14.0", DateTimeOffset.UnixEpoch, [viewport], new PageFacts("tr", true, 3, true, 5, 1, false));

    private static ViewportResult Viewport(IReadOnlyList<RuleViolation> violations) =>
        new("Desktop", 1280, 800, violations);

    private static RuleViolation Violation(string ruleId, string impact, int nodes) =>
        new(ruleId, impact, "Help text", ["wcag2aa", "wcag143"],
            Enumerable.Range(0, nodes).Select(i => new ViolationNode($"#n{i}", "<p></p>")).ToList());
}
