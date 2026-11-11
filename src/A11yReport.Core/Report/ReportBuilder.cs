using A11yReport.Core.Results;
using A11yReport.Core.Sites;

namespace A11yReport.Core.Report;

/// <summary>
/// Turns per-site outcomes into sector-level numbers. Sites that were not scanned (skipped,
/// unreachable, failed, duplicate) are counted in <see cref="Coverage"/> but never treated as
/// pages with zero failures. Opted-out domains are left out completely.
/// </summary>
public static class ReportBuilder
{
    public const int TopRuleCount = 10;
    private const string TurkishLangPrefix = "tr";

    public static ReportData Build(RunInfo run, IEnumerable<SiteOutcome> outcomes, IReadOnlySet<string> optOut)
    {
        var listed = outcomes.Where(site => !optOut.Contains(site.Domain)).ToList();
        var scanned = listed.Where(site => site.Status == ScanStatus.Done).ToList();
        return new ReportData(
            run,
            BuildCoverage(listed, scanned.Count),
            BuildViewport(scanned.Select(site => site.Desktop).ToList()),
            BuildViewport(scanned.Select(site => site.Mobile).ToList()),
            Sectors.All.Select(sector => BuildSector(sector, listed)).ToList(),
            BuildTopRules(scanned),
            StructureBuilder.Build(scanned));
    }

    public static double Percent(int part, int whole) => whole == 0 ? 0 : 100.0 * part / whole;

    public static double Average(IEnumerable<int> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0 : list.Average();
    }

    private static Coverage BuildCoverage(List<SiteOutcome> listed, int scanned)
    {
        var notScanned = listed
            .Where(site => site.Status != ScanStatus.Done)
            .GroupBy(site => site.Status.ToString())
            .OrderByDescending(group => group.Count())
            .ToDictionary(group => group.Key, group => group.Count());
        return new Coverage(listed.Count, scanned, notScanned);
    }

    private static ViewportStats BuildViewport(List<IReadOnlyList<RuleCount>> pages)
    {
        var totals = pages.Select(rules => rules.Sum(rule => rule.Elements)).ToList();
        return new ViewportStats(pages.Count, Percent(totals.Count(total => total > 0), totals.Count), Average(totals));
    }

    private static SectorStats BuildSector(string sector, List<SiteOutcome> listed)
    {
        var inSector = listed.Where(site => site.Sector == sector).ToList();
        var scanned = inSector.Where(site => site.Status == ScanStatus.Done).ToList();
        var desktop = BuildViewport(scanned.Select(site => site.Desktop).ToList());
        return new SectorStats(sector, inSector.Count, scanned.Count, desktop.PercentWithFailures, desktop.AverageFailures);
    }

    private static List<RuleStats> BuildTopRules(List<SiteOutcome> scanned)
    {
        return scanned
            .SelectMany(site => site.Desktop)
            .GroupBy(rule => rule.RuleId)
            .Select(group => new RuleStats(
                group.Key, group.First().Help, Percent(group.Count(), scanned.Count), group.Count(), group.Sum(r => r.Elements)))
            .OrderByDescending(rule => rule.Pages)
            .ThenByDescending(rule => rule.Elements)
            .ThenBy(rule => rule.RuleId, StringComparer.Ordinal)
            .Take(TopRuleCount)
            .ToList();
    }

    internal static bool IsTurkish(string? lang) =>
        lang is not null && lang.Trim().StartsWith(TurkishLangPrefix, StringComparison.OrdinalIgnoreCase);
}
