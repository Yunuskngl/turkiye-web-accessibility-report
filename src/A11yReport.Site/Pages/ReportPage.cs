using System.Globalization;
using System.Text;
using A11yReport.Core.Report;
using A11yReport.Site.Html;
using static A11yReport.Site.Html.HtmlText;

namespace A11yReport.Site.Pages;

/// <summary>The main report page: headline numbers, sectors, common failures, page structure, coverage.</summary>
public sealed class ReportPage(SiteTexts t, RuleNames ruleNames)
{
    private SiteFormat F => t.Format;

    public string Render(ReportData data, bool isPilot, string repositoryUrl)
    {
        var body = new StringBuilder();
        body.Append(Intro(data, isPilot));
        body.Append(KeyNumbers(data));
        body.Append(SectorSection(data));
        body.Append(RuleSection(data));
        body.Append(StructureSection(data.Structure));
        body.Append(CoverageSection(data.Coverage));
        body.Append(Invariant($"""<section aria-labelledby="about"><h2 id="about">{t.AboutHeading}</h2>{t.AboutText}</section>"""));
        return Layout.Page(t, t.ReportTitle, SitePage.Report, body.ToString(), repositoryUrl);
    }

    private string Intro(ReportData data, bool isPilot)
    {
        var lede = SiteTexts.Fill(t.ReportLede, ("pages", F.Count(data.Coverage.Scanned)), ("date", F.Date(data.Run.StartedAt)),
            ("axe", Encode(data.Run.AxeVersion)));
        var pilot = isPilot ? $"""<p class="notice">{t.ReportPilotNotice}</p>""" : string.Empty;
        return Invariant($"<h1>{t.ReportTitle}</h1><p class=\"lede\">{lede}</p>{pilot}");
    }

    private string KeyNumbers(ReportData data) => Invariant($"""
        <section aria-labelledby="key-numbers">
          <h2 id="key-numbers">{t.KeyNumbersHeading}</h2>
          <ul class="stats">
            {Stat(F.Percent(data.Desktop.PercentWithFailures), t.StatDesktopShare)}
            {Stat(F.OneDecimal(data.Desktop.AverageFailures), t.StatDesktopAverage)}
            {Stat(F.Percent(data.Mobile.PercentWithFailures), t.StatMobileShare)}
            {Stat(F.OneDecimal(data.Mobile.AverageFailures), t.StatMobileAverage)}
          </ul>
          <p>{t.FailureDefinition}</p>
        </section>
        """);

    private static string Stat(string value, string label) =>
        $"""<li><span class="stat-value">{value}</span> <span class="stat-label">{label}</span></li>""";

    private string SectorSection(ReportData data)
    {
        var bars = data.Sectors.Where(s => s.Scanned > 0)
            .Select(s => new Bar(SectorName(s.Sector), s.AverageFailures, F.OneDecimal(s.AverageFailures))).ToList();
        var chart = BarChart.Render("sector-chart", t.SectorChartTitle, t.SectorChartDescription, bars, t.SectorChartAxis);
        var rows = string.Concat(data.Sectors.Select(s => Invariant($"""
            <tr><th scope="row">{SectorName(s.Sector)}</th><td>{F.Count(s.Listed)}</td><td>{F.Count(s.Scanned)}</td>
            <td>{(s.Scanned == 0 ? "–" : F.Percent(s.PercentWithFailures))}</td><td>{(s.Scanned == 0 ? "–" : F.OneDecimal(s.AverageFailures))}</td></tr>
            """)));
        return Section("sectors", t.SectorsHeading, $"<figure>{chart}</figure>{Table(t.SectorTableCaption, t.SectorTableHeaders, rows)}");
    }

    private string RuleSection(ReportData data)
    {
        var bars = data.TopRules.Select(r => new Bar(RuleName(r), r.PercentOfPages, F.Percent(r.PercentOfPages))).ToList();
        var chart = BarChart.Render("rule-chart", t.RuleChartTitle, t.RuleChartDescription, bars, t.RuleChartAxis);
        var rows = string.Concat(data.TopRules.Select(r => Invariant($"""
            <tr><th scope="row">{Encode(RuleName(r))} <code>{Encode(r.RuleId)}</code></th>
            <td>{F.Percent(r.PercentOfPages)}</td><td>{F.Count(r.Pages)}</td><td>{F.Count(r.Elements)}</td></tr>
            """)));
        return Section("failures", t.RulesHeading, $"<figure>{chart}</figure>{Table(t.RuleTableCaption, t.RuleTableHeaders, rows)}");
    }

    private string StructureSection(StructureStats s)
    {
        string[] values =
        [
            F.Percent(s.PercentLangMissing), F.Percent(s.PercentLangTurkish), F.Percent(s.PercentWithMain),
            F.Percent(s.PercentExactlyOneH1), F.Percent(s.PercentNoH1), F.Percent(s.PercentSkippedHeadingLevel),
            F.OneDecimal(s.AverageLandmarks), F.OneDecimal(s.AverageHeadings),
        ];
        var rows = string.Concat(t.StructureRows.Zip(values, Row));
        return Section("structure", t.StructureHeading, Table(t.StructureCaption, t.StructureHeaders, rows));
    }

    private string CoverageSection(Coverage coverage)
    {
        var rows = Row(t.CoverageChecked, F.Count(coverage.Scanned)) + string.Concat(
            coverage.NotScannedByStatus.Select(pair => Row(Encode(StatusName(pair.Key)), F.Count(pair.Value))));
        var text = SiteTexts.Fill(t.CoverageText, ("listed", F.Count(coverage.Listed)));
        return Section("coverage", t.CoverageHeading, $"<p>{text}</p>{Table(t.CoverageCaption, t.CoverageHeaders, rows)}");
    }

    private string SectorName(string sector) => t.LangCode == SiteLanguages.Turkish.LangCode ? Labels.SectorTr(sector) : Labels.Sector(sector);

    private string StatusName(string status) => t.LangCode == SiteLanguages.Turkish.LangCode ? Labels.StatusTr(status) : Labels.Status(status);

    private string RuleName(RuleStats rule) => ruleNames.For(t, rule.RuleId, rule.Help);

    private static string Section(string id, string heading, string content) =>
        Invariant($"""<section aria-labelledby="{id}"><h2 id="{id}">{heading}</h2>{content}</section>""");

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
