using System.Globalization;
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
    private const string Title = "Look up a site";

    public static string Render(RunInfo run, bool isPilot, string repositoryUrl)
    {
        var zones = string.Join(' ', DomainKey.TrZoneNames.Order(StringComparer.Ordinal));
        var pilot = isPilot
            ? """<p class="notice"><strong>Pilot results.</strong> Only the sites in the small pilot run can be found.</p>"""
            : string.Empty;
        var body = string.Create(CultureInfo.InvariantCulture, $"""
            <h1>{Title}</h1>
            <p class="lede">See the automated check result for one home page. We show a site's result only when
            someone looks it up; we do not publish a list or a ranking of sites.</p>
            {pilot}
            <form id="lookup-form" class="lookup-form" data-tr-zones="{HtmlText.Encode(zones)}" novalidate>
              <label for="domain">Website address</label>
              <p id="domain-hint" class="hint">For example <code>example.com.tr</code> or <code>https://www.example.com.tr</code></p>
              <div class="lookup-row">
                <input id="domain" name="domain" type="text" inputmode="url" autocomplete="url" spellcheck="false"
                  aria-describedby="domain-hint" required>
                <button type="submit">Look up</button>
              </div>
            </form>
            <p id="lookup-status" class="lookup-status" role="status"></p>
            <div id="lookup-result" class="lookup-result"></div>
            <noscript><p>This search needs JavaScript. All published numbers are also on the report page.</p></noscript>
            <section aria-labelledby="read-first">
              <h2 id="read-first">Read this first</h2>
              <ul>
                <li>Checked on {HtmlText.Date(run.StartedAt)} with axe-core {HtmlText.Encode(run.AxeVersion)}, home page only.</li>
                <li>Automated checks find only part of accessibility barriers. No detected failures does not mean the
                page is accessible, and this is not a legal compliance audit.</li>
                <li>Site owners can ask us to remove their site: see <a href="methodology.html#opting-out">opting out</a>.</li>
              </ul>
            </section>
            <script src="lookup.js" defer></script>
            """);
        return Layout.Page(Title, SitePage.Lookup, body, repositoryUrl);
    }
}
