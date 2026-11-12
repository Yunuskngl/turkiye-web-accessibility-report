using System.Globalization;

namespace A11yReport.Site.Html;

public enum SitePage
{
    Report,
    Lookup,
    Methodology,
}

/// <summary>
/// The shared page frame: language, skip link, header with a language switch and navigation,
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
        return string.Create(CultureInfo.InvariantCulture, $"""
            <!DOCTYPE html>
            <html lang="{t.LangCode}">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{HtmlText.Encode(title)} – {t.SiteName}</title>
              <link rel="stylesheet" href="{RootPrefix(t)}styles.css">
              <link rel="alternate" hreflang="{other.LangCode}" href="{LanguageHref(t, other, current)}">
            </head>
            <body>
              <a class="skip-link" href="#main">{t.SkipLink}</a>
              <header class="site-header">
                <div class="wrap header-top">
                  <p class="site-name"><a href="index.html">{t.SiteName}</a></p>
                  <nav class="lang-switch" aria-label="{t.LanguageLabel}">
                    <ul>{LanguageItems(t, current)}</ul>
                  </nav>
                </div>
                <div class="wrap">
                  <nav class="main-nav" aria-label="{t.NavLabel}">
                    <ul>
                      {NavItem(SitePage.Report, t.NavReport, current)}
                      {NavItem(SitePage.Lookup, t.NavLookup, current)}
                      {NavItem(SitePage.Methodology, t.NavMethodology, current)}
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

    /// <summary>Link to the same page in a language, relative to the current page.</summary>
    private static string LanguageHref(SiteTexts current, SiteTexts target, SitePage page)
    {
        var file = FileName(page);
        if (target.LangCode == current.LangCode)
        {
            return file;
        }

        return target.Folder.Length == 0 ? RootPrefix(current) + file : target.Folder + "/" + file;
    }

    // Both languages are always shown, each written in its own language; the current one is marked.
    private static string LanguageItems(SiteTexts current, SitePage page) =>
        string.Concat(SiteLanguages.All.Select(language => LanguageItem(current, language, page)));

    private static string LanguageItem(SiteTexts current, SiteTexts language, SitePage page)
    {
        var href = LanguageHref(current, language, page);
        var marker = language.LangCode == current.LangCode ? " aria-current=\"true\"" : string.Empty;
        return $"""<li><a href="{href}" lang="{language.LangCode}" hreflang="{language.LangCode}"{marker}>{language.LanguageName}</a></li>""";
    }

    private static string NavItem(SitePage page, string text, SitePage current) => page == current
        ? $"""<li><a href="{FileName(page)}" aria-current="page">{text}</a></li>"""
        : $"""<li><a href="{FileName(page)}">{text}</a></li>""";
}
