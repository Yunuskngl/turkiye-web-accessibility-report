using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using A11yReport.Core.Report;
using A11yReport.Core.Sites;
using A11yReport.Site.Html;

namespace A11yReport.Site.Pages;

/// <summary>
/// Search form for one site's result. The page works without a list of sites: lookup.js turns the
/// typed domain into the same SHA-256 file name the generator used.
/// </summary>
public static class LookupPage
{
    private static readonly JsonSerializerOptions ScriptJson = new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

    public static string Render(SiteTexts t, RunInfo run, bool isPilot, string repositoryUrl)
    {
        var zones = string.Join(' ', DomainKey.TrZoneNames.Order(StringComparer.Ordinal));
        var pilot = isPilot ? $"""<p class="notice">{t.LookupPilotNotice}</p>""" : string.Empty;
        var readFirst = SiteTexts.Fill(t.LookupReadFirst, ("date", t.Format.Date(run.StartedAt)), ("axe", HtmlText.Encode(run.AxeVersion)));
        var root = Layout.RootPrefix(t);
        var body = string.Create(CultureInfo.InvariantCulture, $"""
            <h1>{t.LookupTitle}</h1>
            <p class="lede">{t.LookupLede}</p>
            {pilot}
            <form id="lookup-form" class="lookup-form" data-tr-zones="{HtmlText.Encode(zones)}" data-root="{root}"
              data-lang="{t.LangCode}" novalidate>
              <label for="domain">{t.LookupLabel}</label>
              <p id="domain-hint" class="hint">{t.LookupHint}</p>
              <div class="lookup-row">
                <input id="domain" name="domain" type="text" inputmode="url" autocomplete="url" spellcheck="false"
                  aria-describedby="domain-hint" required>
                <button type="submit">{t.LookupButton}</button>
              </div>
            </form>
            <p id="lookup-status" class="lookup-status" role="status"></p>
            <div id="lookup-result" class="lookup-result"></div>
            <noscript><p>{t.LookupNoScript}</p></noscript>
            <section aria-labelledby="read-first">
              <h2 id="read-first">{t.LookupReadFirstHeading}</h2>
              <ul>{readFirst}</ul>
            </section>
            <script type="application/json" id="lookup-texts">{ScriptTexts(t)}</script>
            <script src="{root}lookup.js" defer></script>
            """);
        return Layout.Page(t, t.LookupTitle, SitePage.Lookup, body, repositoryUrl);
    }

    // JSON inside a script element: "<" is escaped as < by the encoder, so the data cannot close the element.
    private static string ScriptTexts(SiteTexts t) => JsonSerializer.Serialize(t.Script, ScriptJson).Replace("<", "\\u003C", StringComparison.Ordinal);
}
