using A11yReport.Core.Report;
using A11yReport.Core.Results;
using A11yReport.Site.Html;

namespace A11yReport.Site.Lookup;

/// <summary>Builds per-site lookup results. Opted-out sites get no result at all.</summary>
public static class LookupBuilder
{
    private const string Desktop = "Desktop";
    private const string Mobile = "Mobile";
    private const string CheckedText = "Checked";
    private const string CheckedTextTr = "Kontrol edildi";

    public static IReadOnlyList<LookupSite> Build(
        RunInfo run, IEnumerable<SiteOutcome> outcomes, IReadOnlySet<string> optOut, RuleNames names) =>
        outcomes
            .Where(site => !optOut.Contains(site.Domain))
            .Select(site => ToLookup(run, site, names))
            .ToList();

    private static LookupSite ToLookup(RunInfo run, SiteOutcome site, RuleNames names)
    {
        var isChecked = site.Status == ScanStatus.Done;
        IReadOnlyList<LookupViewport> viewports = isChecked
            ? [ToViewport(Desktop, site.Desktop, names), ToViewport(Mobile, site.Mobile, names)]
            : [];
        var status = site.Status.ToString();
        return new LookupSite(
            site.Domain,
            Labels.Sector(site.Sector),
            Labels.SectorTr(site.Sector),
            isChecked,
            isChecked ? CheckedText : Labels.Status(status),
            isChecked ? CheckedTextTr : Labels.StatusTr(status),
            run.StartedAt.ToUniversalTime().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            run.AxeVersion,
            site.LoadEventReached,
            isChecked ? ToFacts(site.Facts) : null,
            viewports);
    }

    private static LookupViewport ToViewport(string name, IReadOnlyList<RuleCount> rules, RuleNames names)
    {
        var lookupRules = rules
            .OrderByDescending(rule => rule.Elements)
            .ThenBy(rule => rule.RuleId, StringComparer.Ordinal)
            .Select(rule => new LookupRule(rule.RuleId, RuleNames.English(rule.RuleId, rule.Help), names.Turkish(rule.RuleId, rule.Help),
                string.Join(", ", rule.Criteria), rule.Elements))
            .ToList();
        return new LookupViewport(name, lookupRules.Count, lookupRules.Sum(rule => rule.Elements), lookupRules);
    }

    private static LookupFacts? ToFacts(PageFacts? facts) => facts is null
        ? null
        : new LookupFacts(facts.Lang, facts.HasTitle, facts.HasMain, facts.H1Count, facts.HeadingCount, facts.SkipsHeadingLevel);
}
