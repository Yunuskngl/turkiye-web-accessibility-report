using A11yReport.Core.Report;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;

namespace A11yReport.Tests;

internal static class ReportSamples
{
    public static readonly RunInfo Run = new("test-run", "Completed", new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero),
        null, "4.14.0", "156", "test list");

    public static SiteOutcome Done(string domain, string sector, params RuleCount[] desktop) =>
        new(domain, sector, ScanStatus.Done, Facts("tr"), desktop, desktop, LoadEventReached: true);

    public static SiteOutcome NotScanned(string domain, string sector, ScanStatus status) =>
        new(domain, sector, status, null, [], [], null);

    public static RuleCount Rule(string ruleId, int elements, string help = "Help text") =>
        new(ruleId, "serious", help, elements);

    public static PageFacts Facts(string? lang, int h1 = 1, bool hasMain = true, bool skips = false) =>
        new(lang, true, 3, hasMain, 5, h1, skips);

    public static ReportData Data() => ReportBuilder.Build(Run,
    [
        Done("a.gov.tr", Sectors.Public, Rule("color-contrast", 10), Rule("image-alt", 2)),
        Done("b.com.tr", Sectors.Ecommerce, Rule("color-contrast", 4)),
        Done("c.com.tr", Sectors.Media),
        NotScanned("d.com.tr", Sectors.Media, ScanStatus.Timeout),
    ], new HashSet<string>());
}
