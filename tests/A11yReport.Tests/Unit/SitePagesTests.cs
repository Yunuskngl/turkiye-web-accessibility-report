using A11yReport.Core.Sites;
using A11yReport.Site.Html;
using A11yReport.Site.Pages;
using static A11yReport.Tests.ReportSamples;

namespace A11yReport.Tests.Unit;

public sealed class SitePagesTests
{
    private const string Repo = "https://github.com/example/repo";

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

        var html = ReportPage.Render(data, isPilot: false, Repo);

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPage_HasLanguageSkipLinkAndPilotNotice()
    {
        var html = ReportPage.Render(Data(), isPilot: true, Repo);

        Assert.Contains("<html lang=\"en\">", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#main\"", html, StringComparison.Ordinal);
        Assert.Contains("Pilot results.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("a.gov.tr", html, StringComparison.Ordinal);
    }

    [Fact]
    public void MethodologyPage_DoesNotPassRawHtmlThrough()
    {
        var html = MethodologyPage.Render("# Title\n\n<script>alert(1)</script>\n\n| a | b |\n|---|---|\n| 1 | 2 |", Repo);

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<table>", html, StringComparison.Ordinal);
    }

    private static A11yReport.Core.Report.ReportData ReportBuilder_WithHelp(string help) =>
        A11yReport.Core.Report.ReportBuilder.Build(Run, [Done("a.com.tr", Sectors.Other, Rule("new-rule", 1, help))],
            new HashSet<string>());
}
