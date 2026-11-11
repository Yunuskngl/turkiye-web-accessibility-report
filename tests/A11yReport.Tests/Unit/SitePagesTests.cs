using A11yReport.Core.Sites;
using A11yReport.Site.Html;
using A11yReport.Site.Pages;
using static A11yReport.Tests.ReportSamples;

namespace A11yReport.Tests.Unit;

public sealed class SitePagesTests
{
    private const string Repo = "https://github.com/example/repo";
    private static readonly RuleNames NoNames = new(new Dictionary<string, string>());

    [Fact]
    public void BarChart_HasAccessibleNameAndEncodesText()
    {
        var svg = BarChart.Render("c1", "Title <b>", "Description", [new Bar("A & B", 5, "5.0")], "Axis");

        Assert.Contains("role=\"img\"", svg, StringComparison.Ordinal);
        Assert.Contains("aria-labelledby=\"c1-title c1-desc\"", svg, StringComparison.Ordinal);
        Assert.Contains("Title &lt;b&gt;", svg, StringComparison.Ordinal);
        Assert.Contains("A &amp; B", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPage_EncodesTextThatComesFromData()
    {
        var data = ReportBuilder_WithHelp("<script>alert(1)</script>");

        var html = new ReportPage(SiteLanguages.English, NoNames).Render(data, isPilot: false, Repo);

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPage_HasLanguageSkipLinkAndPilotNotice()
    {
        var html = new ReportPage(SiteLanguages.English, NoNames).Render(Data(), isPilot: true, Repo);

        Assert.Contains("<html lang=\"en\">", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#main\"", html, StringComparison.Ordinal);
        Assert.Contains("Pilot results.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("a.gov.tr", html, StringComparison.Ordinal);
    }

    [Fact]
    public void MethodologyPage_DoesNotPassRawHtmlThrough()
    {
        var html = MethodologyPage.Render(SiteLanguages.English, "# Title\n\n<script>alert(1)</script>\n\n| a | b |\n|---|---|\n| 1 | 2 |", Repo);

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<table>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void TurkishReportPage_UsesTurkishLanguageNumbersAndRuleNames()
    {
        var names = new RuleNames(new Dictionary<string, string> { ["color-contrast"] = "Düşük kontrastlı metin" });

        var html = new ReportPage(SiteLanguages.Turkish, names).Render(Data(), isPilot: false, Repo);

        Assert.Contains("<html lang=\"tr\">", html, StringComparison.Ordinal);
        Assert.Contains("%75,0", html, StringComparison.Ordinal);
        Assert.Contains("Düşük kontrastlı metin", html, StringComparison.Ordinal);
        Assert.Contains("href=\"../styles.css\"", html, StringComparison.Ordinal);
        Assert.Contains("hreflang=\"en\" href=\"../index.html\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public void EnglishReportPage_LinksToTurkishVersion()
    {
        var html = new ReportPage(SiteLanguages.English, NoNames).Render(Data(), isPilot: false, Repo);

        Assert.Contains("href=\"tr/index.html\" lang=\"tr\"", html, StringComparison.Ordinal);
        Assert.Contains("75.0%", html, StringComparison.Ordinal);
    }

    [Fact]
    public void TurkishMethodology_KeepsTheOptOutAnchor()
    {
        var html = MethodologyPage.Render(SiteLanguages.Turkish, "## Kapsam dışı kalma {#opting-out}\n\nMetin.", Repo);

        Assert.Contains("id=\"opting-out\"", html, StringComparison.Ordinal);
    }

    private static A11yReport.Core.Report.ReportData ReportBuilder_WithHelp(string help) =>
        A11yReport.Core.Report.ReportBuilder.Build(Run, [Done("a.com.tr", Sectors.Other, Rule("new-rule", 1, help))],
            new HashSet<string>());
}
