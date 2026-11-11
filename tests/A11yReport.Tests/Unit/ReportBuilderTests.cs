using A11yReport.Core.Report;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using static A11yReport.Tests.ReportSamples;

namespace A11yReport.Tests.Unit;

public sealed class ReportBuilderTests
{
    private static readonly HashSet<string> NoOptOut = [];

    [Fact]
    public void Build_SitesNotScanned_AreNotCountedAsZeroFailures()
    {
        var data = ReportBuilder.Build(Run,
            [Done("a.com.tr", Sectors.Other, Rule("image-alt", 4)), NotScanned("b.com.tr", Sectors.Other, ScanStatus.Timeout)],
            NoOptOut);

        Assert.Equal(1, data.Desktop.Pages);
        Assert.Equal(100, data.Desktop.PercentWithFailures);
        Assert.Equal(4, data.Desktop.AverageFailures);
        Assert.Equal(1, data.Coverage.NotScannedByStatus["Timeout"]);
    }

    [Fact]
    public void Build_OptedOutSites_AreLeftOutCompletely()
    {
        var data = ReportBuilder.Build(Run,
            [Done("a.com.tr", Sectors.Other, Rule("image-alt", 4)), Done("optout.com.tr", Sectors.Other)],
            new HashSet<string> { "optout.com.tr" });

        Assert.Equal(1, data.Coverage.Listed);
        Assert.Equal(1, data.Desktop.Pages);
    }

    [Fact]
    public void Build_SectorStats_UseOnlyPagesOfThatSector()
    {
        var data = Data();

        var media = data.Sectors.Single(s => s.Sector == Sectors.Media);
        Assert.Equal(2, media.Listed);
        Assert.Equal(1, media.Scanned);
        Assert.Equal(0, media.PercentWithFailures);
        var publicSector = data.Sectors.Single(s => s.Sector == Sectors.Public);
        Assert.Equal(12, publicSector.AverageFailures);
    }

    [Fact]
    public void Build_TopRules_OrderedByShareOfPages()
    {
        var data = Data();

        Assert.Equal(["color-contrast", "image-alt", "link-name"], data.TopRules.Select(r => r.RuleId));
        Assert.Equal(50, data.TopRules[0].PercentOfPages, precision: 6);
        Assert.Equal(14, data.TopRules[0].Elements);
    }

    [Fact]
    public void Build_Structure_CountsTurkishAndMissingLanguage()
    {
        SiteOutcome[] sites =
        [
            Done("a.com.tr", Sectors.Other) with { Facts = Facts("tr-TR") },
            Done("b.com.tr", Sectors.Other) with { Facts = Facts("en", h1: 0) },
            Done("c.com.tr", Sectors.Other) with { Facts = Facts(null, skips: true) },
            Done("d.com.tr", Sectors.Other) with { Facts = Facts("  ") },
        ];

        var structure = ReportBuilder.Build(Run, sites, NoOptOut).Structure;

        Assert.Equal(25, structure.PercentLangTurkish);
        Assert.Equal(50, structure.PercentLangMissing);
        Assert.Equal(25, structure.PercentNoH1);
        Assert.Equal(25, structure.PercentSkippedHeadingLevel);
    }

    [Fact]
    public void Build_NoScannedPages_GivesZeroInsteadOfDividingByZero()
    {
        var data = ReportBuilder.Build(Run, [NotScanned("a.com.tr", Sectors.Other, ScanStatus.DnsError)], NoOptOut);

        Assert.Equal(0, data.Desktop.Pages);
        Assert.Equal(0, data.Desktop.AverageFailures);
    }
}
