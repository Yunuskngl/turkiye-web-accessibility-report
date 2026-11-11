namespace A11yReport.Core.Sites;

/// <summary>Turns host names into one key per site, so www., m. and other subdomains count once.</summary>
public static class DomainKey
{
    private const string WwwPrefix = "www.";

    // Second-level zones under .tr where people register names (TRABIS / Public Suffix List).
    private static readonly HashSet<string> TrZones = new(StringComparer.OrdinalIgnoreCase)
    {
        "av", "bbs", "bel", "biz", "com", "dr", "edu", "gen", "gov", "info", "k12", "kep",
        "mil", "name", "net", "nc", "org", "pol", "tel", "tsk", "tv", "web",
    };

    /// <summary>Northern Cyprus uses nc.tr. This report covers Türkiye only, so it is excluded.</summary>
    public const string ExcludedTrZone = "nc.tr";

    // City top-level domains run for Turkish cities (for example ibb.istanbul).
    private static readonly string[] TurkishCityTlds = [".istanbul", ".ist"];

    public static string FromHost(string host)
    {
        var labels = Normalize(host).Split('.');
        if (labels.Length <= 2)
        {
            return string.Join('.', labels);
        }

        var isTrZone = labels[^1] == "tr" && TrZones.Contains(labels[^2]);
        var keep = isTrZone ? 3 : 2;
        return string.Join('.', labels[^keep..]);
    }

    public static bool IsTurkishTld(string domain)
    {
        var normalized = Normalize(domain);
        if (normalized.EndsWith("." + ExcludedTrZone, StringComparison.Ordinal))
        {
            return false;
        }

        return normalized.EndsWith(".tr", StringComparison.Ordinal)
            || TurkishCityTlds.Any(tld => normalized.EndsWith(tld, StringComparison.Ordinal));
    }

    public static bool IsHomeHostOf(string host, string domain)
    {
        var normalized = host.Trim().TrimEnd('.').ToLowerInvariant();
        return normalized == domain || normalized == WwwPrefix + domain;
    }

    private static string Normalize(string host)
    {
        var normalized = host.Trim().TrimEnd('.').ToLowerInvariant();
        return normalized.StartsWith(WwwPrefix, StringComparison.Ordinal) ? normalized[WwwPrefix.Length..] : normalized;
    }
}
