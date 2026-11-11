namespace A11yReport.Core.Sites;

public static class Sectors
{
    public const string Public = "public";
    public const string Finance = "finance";
    public const string Ecommerce = "ecommerce";
    public const string University = "university";
    public const string Media = "media";
    public const string Other = "other";

    public static readonly IReadOnlyList<string> All = [Public, Finance, Ecommerce, University, Media, Other];

    // Zones that only public bodies can register. Municipalities (.bel.tr) belong to the public sector.
    private static readonly string[] PublicZones = [".gov.tr", ".bel.tr", ".pol.tr", ".mil.tr", ".tsk.tr"];
    private const string UniversityZone = ".edu.tr";

    public static bool IsKnown(string sector) => All.Contains(sector);

    /// <summary>Sector from the domain name alone, or null when a person has to decide.</summary>
    public static string? FromRules(string domain)
    {
        if (PublicZones.Any(zone => domain.EndsWith(zone, StringComparison.OrdinalIgnoreCase)))
        {
            return Public;
        }

        return domain.EndsWith(UniversityZone, StringComparison.OrdinalIgnoreCase) ? University : null;
    }

    public static bool IsPublicZone(string domain) =>
        PublicZones.Any(zone => domain.EndsWith(zone, StringComparison.OrdinalIgnoreCase));
}
