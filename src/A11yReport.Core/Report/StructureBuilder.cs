using A11yReport.Core.Results;

namespace A11yReport.Core.Report;

internal static class StructureBuilder
{
    public static StructureStats Build(List<SiteOutcome> scanned)
    {
        var facts = scanned.Select(site => site.Facts).OfType<PageFacts>().ToList();
        var loadRecords = scanned.Select(site => site.LoadEventReached).OfType<bool>().ToList();
        int Count(Func<PageFacts, bool> test) => facts.Count(test);

        return new StructureStats(
            facts.Count,
            ReportBuilder.Percent(Count(f => string.IsNullOrWhiteSpace(f.Lang)), facts.Count),
            ReportBuilder.Percent(Count(f => ReportBuilder.IsTurkish(f.Lang)), facts.Count),
            ReportBuilder.Percent(Count(f => f.HasMain), facts.Count),
            ReportBuilder.Percent(Count(f => f.H1Count == 1), facts.Count),
            ReportBuilder.Percent(Count(f => f.H1Count == 0), facts.Count),
            ReportBuilder.Percent(Count(f => f.SkipsHeadingLevel), facts.Count),
            ReportBuilder.Average(facts.Select(f => f.LandmarkCount)),
            ReportBuilder.Average(facts.Select(f => f.HeadingCount)),
            loadRecords.Count,
            ReportBuilder.Percent(loadRecords.Count(reached => !reached), loadRecords.Count));
    }
}
