namespace A11yReport.Site.Html;

/// <summary>
/// All fixed text of the site in one language. Values may contain simple trusted HTML
/// (strong, code, links); text from data is never put in here.
/// </summary>
public sealed record SiteTexts
{
    public required string LangCode { get; init; }
    public required string Folder { get; init; }
    public required SiteFormat Format { get; init; }

    public required string SiteName { get; init; }
    public required string SkipLink { get; init; }
    public required string NavLabel { get; init; }
    public required string NavReport { get; init; }
    public required string NavLookup { get; init; }
    public required string NavMethodology { get; init; }
    public required string OtherLanguageName { get; init; }
    public required string FooterText { get; init; }
    public required string FooterRepository { get; init; }
    public required string FooterOptOut { get; init; }

    public required string ReportTitle { get; init; }
    public required string ReportLede { get; init; }
    public required string ReportPilotNotice { get; init; }
    public required string KeyNumbersHeading { get; init; }
    public required string StatDesktopShare { get; init; }
    public required string StatDesktopAverage { get; init; }
    public required string StatMobileShare { get; init; }
    public required string StatMobileAverage { get; init; }
    public required string FailureDefinition { get; init; }
    public required string SectorsHeading { get; init; }
    public required string SectorChartTitle { get; init; }
    public required string SectorChartDescription { get; init; }
    public required string SectorChartAxis { get; init; }
    public required string SectorTableCaption { get; init; }
    public required IReadOnlyList<string> SectorTableHeaders { get; init; }
    public required string RulesHeading { get; init; }
    public required string RuleChartTitle { get; init; }
    public required string RuleChartDescription { get; init; }
    public required string RuleChartAxis { get; init; }
    public required string RuleTableCaption { get; init; }
    public required IReadOnlyList<string> RuleTableHeaders { get; init; }
    public required string StructureHeading { get; init; }
    public required string StructureCaption { get; init; }
    public required IReadOnlyList<string> StructureHeaders { get; init; }
    public required IReadOnlyList<string> StructureRows { get; init; }
    public required string CoverageHeading { get; init; }
    public required string CoverageText { get; init; }
    public required string CoverageCaption { get; init; }
    public required IReadOnlyList<string> CoverageHeaders { get; init; }
    public required string CoverageChecked { get; init; }
    public required string AboutHeading { get; init; }
    public required string AboutText { get; init; }

    public required string LookupTitle { get; init; }
    public required string LookupLede { get; init; }
    public required string LookupPilotNotice { get; init; }
    public required string LookupLabel { get; init; }
    public required string LookupHint { get; init; }
    public required string LookupButton { get; init; }
    public required string LookupNoScript { get; init; }
    public required string LookupReadFirstHeading { get; init; }
    public required string LookupReadFirst { get; init; }

    /// <summary>Strings used by lookup.js, passed to the page as JSON.</summary>
    public required IReadOnlyDictionary<string, string> Script { get; init; }

    public static string Fill(string template, params (string Key, string Value)[] values) =>
        values.Aggregate(template, (text, pair) => text.Replace("{" + pair.Key + "}", pair.Value, StringComparison.Ordinal));
}
