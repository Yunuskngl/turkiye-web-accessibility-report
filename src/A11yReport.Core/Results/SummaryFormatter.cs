using System.Globalization;
using System.Text;

namespace A11yReport.Core.Results;

public static class SummaryFormatter
{
    private const string Limitation =
        "Note: automated checks find only part of accessibility problems. " +
        "This is not a compliance verdict.";

    public static string Build(PageScanResult result)
    {
        var text = new StringBuilder();
        AppendHeader(text, result);
        foreach (var viewport in result.Viewports)
        {
            text.AppendLine();
            AppendViewport(text, viewport);
        }

        text.AppendLine();
        text.AppendLine(Limitation);
        return text.ToString();
    }

    private static void AppendHeader(StringBuilder text, PageScanResult result)
    {
        var scannedAt = result.ScannedAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
        text.AppendLine(CultureInfo.InvariantCulture, $"URL:        {result.RequestedUrl}");
        text.AppendLine(CultureInfo.InvariantCulture, $"Final URL:  {result.FinalUrl} (HTTP {result.HttpStatus})");
        text.AppendLine(CultureInfo.InvariantCulture, $"Scanned at: {scannedAt}");
        text.AppendLine(CultureInfo.InvariantCulture, $"Engine:     axe-core {result.AxeVersion} (WCAG 2.0/2.1/2.2 A and AA rules)");
    }

    private static void AppendViewport(StringBuilder text, ViewportResult viewport)
    {
        text.AppendLine(CultureInfo.InvariantCulture,
            $"{viewport.Name} ({viewport.Width}x{viewport.Height}): " +
            $"{viewport.Violations.Count} failed rules, {viewport.NodeCount} elements");

        if (viewport.Violations.Count == 0)
        {
            text.AppendLine("  No failures detected by the automated rules.");
            return;
        }

        text.AppendLine("  Impact     Rule                          Elements  WCAG");
        foreach (var violation in SortBySeverity(viewport.Violations))
        {
            AppendViolation(text, violation);
        }
    }

    private static void AppendViolation(StringBuilder text, RuleViolation violation)
    {
        var criteria = string.Join(", ", WcagCriteria.FromTags(violation.Tags));
        text.AppendLine(CultureInfo.InvariantCulture,
            $"  {violation.Impact,-10} {violation.RuleId,-29} {violation.NodeCount,8}  {criteria}");
        text.AppendLine(CultureInfo.InvariantCulture, $"             {violation.Help}");
    }

    private static IEnumerable<RuleViolation> SortBySeverity(IEnumerable<RuleViolation> violations) =>
        violations
            .OrderBy(violation => Impact.Rank(violation.Impact))
            .ThenByDescending(violation => violation.NodeCount)
            .ThenBy(violation => violation.RuleId, StringComparer.Ordinal);
}
