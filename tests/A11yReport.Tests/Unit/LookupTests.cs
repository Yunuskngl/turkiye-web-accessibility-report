using System.Text.Json;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Site.Lookup;
using static A11yReport.Tests.ReportSamples;

namespace A11yReport.Tests.Unit;

public sealed class LookupTests
{
    [Fact]
    public void FileNameFor_IsLowercaseSha256OfDomain() =>
        // Expected value from: printf 'example.com.tr' | shasum -a 256
        Assert.Equal("28b050da874df7d2c4ef8c2b6bfe1706e2b060a3938d115aaef37eb8ce0b01fd.json",
            LookupFiles.FileNameFor("example.com.tr"));

    [Fact]
    public void Build_LeavesOutOptedOutSites()
    {
        var sites = LookupBuilder.Build(Run, Outcomes(), new HashSet<string> { "b.com.tr" });

        Assert.DoesNotContain(sites, site => site.Domain == "b.com.tr");
        Assert.Equal(Outcomes().Count - 1, sites.Count);
    }

    [Fact]
    public void Build_NotCheckedSite_HasStatusButNoNumbers()
    {
        var site = LookupBuilder.Build(Run, [NotScanned("d.com.tr", Sectors.Media, ScanStatus.SkippedRobots)], new HashSet<string>()).Single();

        Assert.False(site.Checked);
        Assert.Equal("Skipped: robots.txt does not allow it", site.StatusText);
        Assert.Empty(site.Viewports);
        Assert.Null(site.Facts);
    }

    [Fact]
    public void Build_CheckedSite_HasRuleNamesCriteriaAndCountsButNoMarkup()
    {
        var site = LookupBuilder.Build(Run, Outcomes(), new HashSet<string>()).Single(s => s.Domain == "a.gov.tr");

        var desktop = site.Viewports[0];
        Assert.Equal(2, desktop.FailedRules);
        Assert.Equal(12, desktop.Elements);
        Assert.Equal(new LookupRule("color-contrast", "Low contrast text", "1.4.3", 10), desktop.Rules[0]);
        var json = JsonSerializer.Serialize(site);
        Assert.DoesNotContain("snippet", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("selector", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WriteAll_RemovesFilesOfSitesThatAreNoLongerPublished()
    {
        var folder = Directory.CreateTempSubdirectory("a11yreport-lookup-").FullName;
        try
        {
            LookupFiles.WriteAll(folder, LookupBuilder.Build(Run, Outcomes(), new HashSet<string>()));
            LookupFiles.WriteAll(folder, LookupBuilder.Build(Run, Outcomes(), new HashSet<string> { "b.com.tr" }));

            Assert.False(File.Exists(Path.Combine(folder, LookupFiles.FolderName, LookupFiles.FileNameFor("b.com.tr"))));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
