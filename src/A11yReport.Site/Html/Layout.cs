using System.Globalization;

namespace A11yReport.Site.Html;

public enum SitePage
{
    Report,
    Methodology,
}

/// <summary>The shared page frame: language, skip link, header navigation, main landmark and footer.</summary>
public static class Layout
{
    public const string SiteName = "Türkiye Web Accessibility Report";

    public static string Page(string title, SitePage current, string body, string repositoryUrl) =>
        string.Create(CultureInfo.InvariantCulture, $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{HtmlText.Encode(title)} – {SiteName}</title>
              <link rel="stylesheet" href="styles.css">
            </head>
            <body>
              <a class="skip-link" href="#main">Skip to main content</a>
              <header class="site-header">
                <div class="wrap">
                  <p class="site-name"><a href="index.html">{SiteName}</a></p>
                  <nav aria-label="Main">
                    <ul>
                      {NavItem("index.html", "Report", current == SitePage.Report)}
                      {NavItem("methodology.html", "Methodology", current == SitePage.Methodology)}
                    </ul>
                  </nav>
                </div>
              </header>
              <main id="main" class="wrap">
            {body}
              </main>
              <footer class="site-footer">
                <div class="wrap">
                  <p>Automated results from axe-core. Not a legal compliance audit. Source code and data:
                    <a href="{HtmlText.Encode(repositoryUrl)}">GitHub repository</a>.</p>
                </div>
              </footer>
            </body>
            </html>
            """);

    private static string NavItem(string href, string text, bool isCurrent) => isCurrent
        ? $"""<li><a href="{href}" aria-current="page">{text}</a></li>"""
        : $"""<li><a href="{href}">{text}</a></li>""";
}
