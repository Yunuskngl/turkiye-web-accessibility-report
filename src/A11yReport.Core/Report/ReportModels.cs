namespace A11yReport.Core.Report;

/// <summary>Aggregated, publishable numbers for one run. Sector level only, no site names.</summary>
public sealed record ReportData(
    RunInfo Run,
    Coverage Coverage,
    ViewportStats Desktop,
    ViewportStats Mobile,
    IReadOnlyList<SectorStats> Sectors,
    IReadOnlyList<RuleStats> TopRules,
    StructureStats Structure);

/// <summary>How many listed sites ended in each status. Only "Done" sites count in the statistics.</summary>
public sealed record Coverage(int Listed, int Scanned, IReadOnlyDictionary<string, int> NotScannedByStatus);

/// <summary>Detected failures in one viewport. A failure is one page element that fails one axe rule.</summary>
public sealed record ViewportStats(int Pages, double PercentWithFailures, double AverageFailures);

public sealed record SectorStats(string Sector, int Listed, int Scanned, double PercentWithFailures, double AverageFailures);

public sealed record RuleStats(string RuleId, string Help, double PercentOfPages, int Pages, int Elements);

public sealed record StructureStats(
    int Pages,
    double PercentLangMissing,
    double PercentLangTurkish,
    double PercentWithMain,
    double PercentExactlyOneH1,
    double PercentNoH1,
    double PercentSkippedHeadingLevel,
    double AverageLandmarks,
    double AverageHeadings,
    int PagesWithLoadRecord,
    double PercentScannedBeforeLoadEvent);
