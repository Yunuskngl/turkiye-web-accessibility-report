using A11yReport.Core.Sites;

namespace A11yReport.Site.Html;

/// <summary>Plain-language names for sectors, statuses and rules, in English and Turkish.</summary>
public static class Labels
{
    private static readonly Dictionary<string, (string En, string Tr)> SectorNames = new()
    {
        [Sectors.Public] = ("Public sector", "Kamu"),
        [Sectors.Finance] = ("Banking and finance", "Bankacılık ve finans"),
        [Sectors.Ecommerce] = ("E-commerce", "E-ticaret"),
        [Sectors.University] = ("Universities", "Üniversiteler"),
        [Sectors.Media] = ("News and media", "Haber ve medya"),
        [Sectors.Other] = ("Other", "Diğer"),
    };

    private static readonly Dictionary<string, (string En, string Tr)> StatusNames = new()
    {
        ["SkippedRobots"] = ("Skipped: robots.txt does not allow it", "Atlandı: robots.txt izin vermiyor"),
        ["Unreachable"] = ("Refused the request or unreachable", "İsteği reddetti veya erişilemedi"),
        ["Timeout"] = ("Did not load in time", "Zamanında yüklenmedi"),
        ["DnsError"] = ("Domain name not found", "Alan adı bulunamadı"),
        ["TlsError"] = ("Secure connection failed", "Güvenli bağlantı kurulamadı"),
        ["NetworkError"] = ("Network error", "Ağ hatası"),
        ["HttpError"] = ("Server returned an error page", "Sunucu hata sayfası döndürdü"),
        ["NonHtml"] = ("Not an HTML page", "HTML sayfası değil"),
        ["Duplicate"] = ("Redirects to another listed site", "Listedeki başka bir siteye yönleniyor"),
        ["Failed"] = ("Check could not run", "Kontrol çalıştırılamadı"),
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

    public static string Sector(string sector) => SectorNames.TryGetValue(sector, out var name) ? name.En : sector;

    public static string SectorTr(string sector) => SectorNames.TryGetValue(sector, out var name) ? name.Tr : sector;

    public static string Status(string status) => StatusNames.TryGetValue(status, out var name) ? name.En : status;

    public static string StatusTr(string status) => StatusNames.TryGetValue(status, out var name) ? name.Tr : status;

    /// <summary>Our plain name for common rules; axe's own one-line help text for the rest.</summary>
    public static string Rule(string ruleId, string help) => RuleNames.GetValueOrDefault(ruleId, help);
}
