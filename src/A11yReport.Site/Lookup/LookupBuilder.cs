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

    public static IReadOnlyList<LookupSite> Build(RunInfo run, IEnumerable<SiteOutcome> outcomes, IReadOnlySet<string> optOut) =>
        outcomes
            .Where(site => !optOut.Contains(site.Domain))
            .Select(site => ToLookup(run, site))
            .ToList();

    private static LookupSite ToLookup(RunInfo run, SiteOutcome site)
    {
        var isChecked = site.Status == ScanStatus.Done;
        IReadOnlyList<LookupViewport> viewports = isChecked
            ? [ToViewport(Desktop, site.Desktop), ToViewport(Mobile, site.Mobile)]
            : [];
        return new LookupSite(
            site.Domain,
            Labels.Sector(site.Sector),
            isChecked,
            isChecked ? CheckedText : Labels.Status(site.Status.ToString()),
            HtmlText.Date(run.StartedAt),
            run.AxeVersion,
            site.LoadEventReached,
            isChecked ? ToFacts(site.Facts) : null,
            viewports);
    }

    private static LookupViewport ToViewport(string name, IReadOnlyList<RuleCount> rules)
    {
        var lookupRules = rules
            .OrderByDescending(rule => rule.Elements)
            .ThenBy(rule => rule.RuleId, StringComparer.Ordinal)
            .Select(rule => new LookupRule(rule.RuleId, Labels.Rule(rule.RuleId, rule.Help), string.Join(", ", rule.Criteria), rule.Elements))
            .ToList();
        return new LookupViewport(name, lookupRules.Count, lookupRules.Sum(rule => rule.Elements), lookupRules);
    }

    private static LookupFacts? ToFacts(PageFacts? facts) => facts is null
        ? null
        : new LookupFacts(facts.Lang, facts.HasTitle, facts.HasMain, facts.H1Count, facts.HeadingCount, facts.SkipsHeadingLevel);
}
