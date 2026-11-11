using System.Globalization;

namespace A11yReport.Site.Html;

public enum SitePage
{
    Report,
    Lookup,
    Methodology,
}

/// <summary>
/// The shared page frame: language, skip link, header navigation with a language switch,
/// main landmark and footer. English pages live at the root, Turkish pages in /tr/.
/// </summary>
public static class Layout
{
    public static string FileName(SitePage page) => page switch
    {
        SitePage.Lookup => "lookup.html",
        SitePage.Methodology => "methodology.html",
        _ => "index.html",
    };

    /// <summary>Relative path from a page in this language to the site root ("" or "../").</summary>
    public static string RootPrefix(SiteTexts texts) => texts.Folder.Length == 0 ? string.Empty : "../";

    public static string Page(SiteTexts t, string title, SitePage current, string body, string repositoryUrl)
    {
        var other = t.LangCode == SiteLanguages.English.LangCode ? SiteLanguages.Turkish : SiteLanguages.English;
        var otherHref = OtherLanguageHref(t, other, current);
        return string.Create(CultureInfo.InvariantCulture, $"""
            <!DOCTYPE html>
            <html lang="{t.LangCode}">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{HtmlText.Encode(title)} – {t.SiteName}</title>
              <link rel="stylesheet" href="{RootPrefix(t)}styles.css">
              <link rel="alternate" hreflang="{other.LangCode}" href="{otherHref}">
            </head>
            <body>
              <a class="skip-link" href="#main">{t.SkipLink}</a>
              <header class="site-header">
                <div class="wrap">
                  <p class="site-name"><a href="index.html">{t.SiteName}</a></p>
                  <nav aria-label="{t.NavLabel}">
                    <ul>
                      {NavItem(SitePage.Report, t.NavReport, current)}
                      {NavItem(SitePage.Lookup, t.NavLookup, current)}
                      {NavItem(SitePage.Methodology, t.NavMethodology, current)}
                      <li><a href="{otherHref}" lang="{other.LangCode}" hreflang="{other.LangCode}">{t.OtherLanguageName}</a></li>
                    </ul>
                  </nav>
                </div>
              </header>
              <main id="main" class="wrap">
            {body}
              </main>
              <footer class="site-footer">
                <div class="wrap">
                  <p>{t.FooterText} <a href="{HtmlText.Encode(repositoryUrl)}">{t.FooterRepository}</a>. {t.FooterOptOut}</p>
                </div>
              </footer>
            </body>
            </html>
            """);
    }

    private static string OtherLanguageHref(SiteTexts current, SiteTexts other, SitePage page)
    {
        var file = FileName(page);
        return other.Folder.Length == 0 ? RootPrefix(current) + file : other.Folder + "/" + file;
    }

    private static string NavItem(SitePage page, string text, SitePage current) => page == current
        ? $"""<li><a href="{FileName(page)}" aria-current="page">{text}</a></li>"""
        : $"""<li><a href="{FileName(page)}">{text}</a></li>""";
}
