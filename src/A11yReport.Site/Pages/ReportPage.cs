using System.Globalization;
using System.Text;
using A11yReport.Core.Report;
using A11yReport.Site.Html;
using static A11yReport.Site.Html.HtmlText;

namespace A11yReport.Site.Pages;

/// <summary>The main report page: headline numbers, sectors, common failures, page structure, coverage.</summary>
public static class ReportPage
{
    private const string Title = "Accessibility of Türkiye's most visited home pages";

    public static string Render(ReportData data, bool isPilot, string repositoryUrl)
    {
        var body = new StringBuilder();
        body.Append(Intro(data, isPilot));
        body.Append(KeyNumbers(data));
        body.Append(SectorSection(data));
        body.Append(RuleSection(data));
        body.Append(StructureSection(data.Structure));
        body.Append(CoverageSection(data.Coverage));
        body.Append(AboutSection());
        return Layout.Page(Title, SitePage.Report, body.ToString(), repositoryUrl);
    }

    private static string Intro(ReportData data, bool isPilot)
    {
        var pilot = isPilot
            ? """<p class="notice"><strong>Pilot results.</strong> These numbers come from a small pilot run used to test the method. They are not the final report.</p>"""
            : string.Empty;
        return Invariant($"""
            <h1>{Title}</h1>
            <p class="lede">An automated check of {Count(data.Coverage.Scanned)} home pages of popular websites in Türkiye
            for WCAG 2.2 Level A and AA failures that software can detect. Checked on {Date(data.Run.StartedAt)}
            with axe-core {Encode(data.Run.AxeVersion)}.</p>
            {pilot}
            """);
    }

    private static string KeyNumbers(ReportData data) => Invariant($"""
        <section aria-labelledby="key-numbers">
          <h2 id="key-numbers">Key numbers</h2>
          <ul class="stats">
            {Stat(Percent(data.Desktop.PercentWithFailures), "of home pages had at least one detected failure (desktop view)")}
            {Stat(OneDecimal(data.Desktop.AverageFailures), "detected failures per home page on average (desktop view)")}
            {Stat(Percent(data.Mobile.PercentWithFailures), "of home pages had at least one detected failure (mobile view)")}
            {Stat(OneDecimal(data.Mobile.AverageFailures), "detected failures per home page on average (mobile view)")}
          </ul>
          <p>A <strong>detected failure</strong> is one element on a page that fails one automated WCAG check, for
          example one image without alternative text. Automated checks find only part of the barriers people meet,
          so a page with no detected failures is not necessarily accessible.</p>
        </section>
        """);

    private static string Stat(string value, string label) =>
        $"""<li><span class="stat-value">{value}</span> <span class="stat-label">{label}</span></li>""";

    private static string SectorSection(ReportData data)
    {
        var scanned = data.Sectors.Where(s => s.Scanned > 0).ToList();
        var bars = scanned.Select(s => new Bar(Labels.Sector(s.Sector), s.AverageFailures, OneDecimal(s.AverageFailures))).ToList();
        var chart = BarChart.Render("sector-chart", "Average detected failures per home page, by sector",
            "Bar chart of the average number of detected failures per home page in each sector, desktop view. "
            + "The same numbers are in the table below.", bars, "Average detected failures per home page (desktop)");
        return Invariant($"""
            <section aria-labelledby="sectors">
              <h2 id="sectors">By sector</h2>
              <figure>{chart}</figure>
              {SectorTable(data.Sectors)}
            </section>
            """);
    }

    private static string SectorTable(IReadOnlyList<SectorStats> sectors)
    {
        var rows = string.Concat(sectors.Select(s => Invariant($"""
            <tr><th scope="row">{Labels.Sector(s.Sector)}</th><td>{Count(s.Listed)}</td><td>{Count(s.Scanned)}</td>
            <td>{(s.Scanned == 0 ? "–" : Percent(s.PercentWithFailures))}</td><td>{(s.Scanned == 0 ? "–" : OneDecimal(s.AverageFailures))}</td></tr>
            """)));
        return Table("Detected failures by sector, desktop view",
            ["Sector", "Sites in list", "Pages checked", "Pages with a detected failure", "Average detected failures per page"],
            rows);
    }

    private static string RuleSection(ReportData data)
    {
        var bars = data.TopRules.Select(r => new Bar(Labels.Rule(r.RuleId, r.Help), r.PercentOfPages, Percent(r.PercentOfPages))).ToList();
        var chart = BarChart.Render("rule-chart", "Most common detected failures",
            "Bar chart of the share of checked home pages with each type of failure, desktop view. "
            + "The same numbers are in the table below.", bars, "Share of checked home pages (desktop)");
        var rows = string.Concat(data.TopRules.Select(r => Invariant($"""
            <tr><th scope="row">{Encode(Labels.Rule(r.RuleId, r.Help))} <code>{Encode(r.RuleId)}</code></th>
            <td>{Percent(r.PercentOfPages)}</td><td>{Count(r.Pages)}</td><td>{Count(r.Elements)}</td></tr>
            """)));
        return Invariant($"""
            <section aria-labelledby="failures">
              <h2 id="failures">Most common failures</h2>
              <figure>{chart}</figure>
              {Table("Most common detected failures, desktop view", ["Failure type (axe rule)", "Share of pages", "Pages", "Elements"], rows)}
            </section>
            """);
    }

    private static string StructureSection(StructureStats s) => Invariant($"""
        <section aria-labelledby="structure">
          <h2 id="structure">Language, landmarks and headings</h2>
          {Table("Page language and structure", ["Measure", "Home pages"], string.Concat(
              Row("Page language not set (<code>lang</code> attribute missing or empty)", Percent(s.PercentLangMissing)),
              Row("Page language set to Turkish (<code>lang=\"tr\"</code> or <code>tr-TR</code>)", Percent(s.PercentLangTurkish)),
              Row("Has a <code>main</code> landmark", Percent(s.PercentWithMain)),
              Row("Exactly one first-level heading (<code>h1</code>)", Percent(s.PercentExactlyOneH1)),
              Row("No first-level heading", Percent(s.PercentNoH1)),
              Row("Skips a heading level (for example h2 to h4)", Percent(s.PercentSkippedHeadingLevel)),
              Row("Average number of landmarks", OneDecimal(s.AverageLandmarks)),
              Row("Average number of headings", OneDecimal(s.AverageHeadings))))}
        </section>
        """);

    private static string CoverageSection(Coverage coverage)
    {
        var rows = Row("Checked", Count(coverage.Scanned)) + string.Concat(
            coverage.NotScannedByStatus.Select(pair => Row(Encode(Labels.Status(pair.Key)), Count(pair.Value))));
        return Invariant($"""
            <section aria-labelledby="coverage">
              <h2 id="coverage">Which sites were checked</h2>
              <p>{Count(coverage.Listed)} sites were in the list. Sites that could not be checked are listed here
              and are <strong>not</strong> counted as pages without failures.</p>
              {Table("Outcome for every site in the list", ["Outcome", "Sites"], rows)}
            </section>
            """);
    }

    private static string AboutSection() => """
        <section aria-labelledby="about">
          <h2 id="about">About this report</h2>
          <p>This report shows sector-level results only. It does not rank websites and it is not a legal
          compliance audit. Our numbers come from the axe-core engine and are not directly comparable with
          reports that use other tools, such as the WebAIM Million.</p>
          <p>Read how the sites were chosen and checked on the <a href="methodology.html">methodology page</a>.</p>
        </section>
        """;

    private static string Row(string label, string value) => $"<tr><th scope=\"row\">{label}</th><td>{value}</td></tr>";

    private static string Table(string caption, IReadOnlyList<string> headers, string rows)
    {
        var head = string.Concat(headers.Select(h => $"<th scope=\"col\">{Encode(h)}</th>"));

        // On narrow screens the table scrolls sideways, so keyboard users must be able to focus it (WCAG 2.1.1).
        return Invariant($"""
            <div class="table-wrap" role="region" tabindex="0" aria-label="{Encode(caption)}"><table>
              <caption>{Encode(caption)}</caption>
              <thead><tr>{head}</tr></thead>
              <tbody>{rows}</tbody>
            </table></div>
            """);
    }

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
