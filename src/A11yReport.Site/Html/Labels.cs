using A11yReport.Core.Sites;

namespace A11yReport.Site.Html;

/// <summary>Plain-language names for sectors, statuses and the most common axe rules.</summary>
public static class Labels
{
    private static readonly Dictionary<string, string> SectorNames = new()
    {
        [Sectors.Public] = "Public sector",
        [Sectors.Finance] = "Banking and finance",
        [Sectors.Ecommerce] = "E-commerce",
        [Sectors.University] = "Universities",
        [Sectors.Media] = "News and media",
        [Sectors.Other] = "Other",
    };

    private static readonly Dictionary<string, string> StatusNames = new()
    {
        ["SkippedRobots"] = "Skipped: robots.txt does not allow it",
        ["Unreachable"] = "Refused the request or unreachable",
        ["Timeout"] = "Did not load in time",
        ["DnsError"] = "Domain name not found",
        ["TlsError"] = "Secure connection failed",
        ["NetworkError"] = "Network error",
        ["HttpError"] = "Server returned an error page",
        ["NonHtml"] = "Not an HTML page",
        ["Duplicate"] = "Redirects to another listed site",
        ["Failed"] = "Check could not run",
    };

    private static readonly Dictionary<string, string> RuleNames = new()
    {
        ["color-contrast"] = "Low contrast text",
        ["image-alt"] = "Images without alternative text",
        ["link-name"] = "Links without a text name",
        ["button-name"] = "Buttons without a text name",
        ["label"] = "Form fields without a label",
        ["select-name"] = "Drop-down lists without a label",
        ["html-has-lang"] = "Page language not set",
        ["html-lang-valid"] = "Invalid page language",
        ["document-title"] = "Missing page title",
        ["frame-title"] = "Frames without a title",
        ["target-size"] = "Touch targets too small",
        ["meta-viewport"] = "Zooming is blocked",
        ["list"] = "Incorrect list structure",
        ["listitem"] = "List items outside a list",
        ["aria-allowed-attr"] = "ARIA attributes not allowed on the element",
        ["aria-prohibited-attr"] = "Prohibited ARIA attributes",
        ["aria-hidden-focus"] = "Hidden elements that can still get focus",
        ["aria-required-children"] = "ARIA roles missing required children",
        ["aria-required-parent"] = "ARIA roles missing a required parent",
        ["label-content-name-mismatch"] = "Visible label differs from accessible name",
        ["nested-interactive"] = "Interactive controls nested inside each other",
        ["role-img-alt"] = "Image roles without alternative text",
        ["avoid-inline-spacing"] = "Text spacing cannot be adjusted",
    };

    public static string Sector(string sector) => SectorNames.GetValueOrDefault(sector, sector);

    public static string Status(string status) => StatusNames.GetValueOrDefault(status, status);

    /// <summary>Our plain name for common rules; axe's own one-line help text for the rest.</summary>
    public static string Rule(string ruleId, string help) => RuleNames.GetValueOrDefault(ruleId, help);
}
