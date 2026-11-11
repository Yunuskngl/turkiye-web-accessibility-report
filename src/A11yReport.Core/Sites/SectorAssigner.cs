namespace A11yReport.Core.Sites;

public static class SectorAssigner
{
    /// <summary>
    /// Keeps every existing (hand-reviewed) row and adds rows for new domains:
    /// the rule-based sector when a rule applies, otherwise "other" marked for review.
    /// </summary>
    public static IReadOnlyList<SectorAssignment> AddMissing(
        IReadOnlyList<SectorAssignment> existing, IEnumerable<string> domains)
    {
        var known = existing.Select(a => a.Domain).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = domains
            .Where(domain => !known.Contains(domain))
            .Select(ByRule);
        return [.. existing, .. added];
    }

    public static string SectorOf(string domain, IReadOnlyList<SectorAssignment> assignments)
    {
        var match = assignments.FirstOrDefault(a => string.Equals(a.Domain, domain, StringComparison.OrdinalIgnoreCase));
        return match is not null && Sectors.IsKnown(match.Sector) ? match.Sector : Sectors.FromRules(domain) ?? Sectors.Other;
    }

    private static SectorAssignment ByRule(string domain)
    {
        var sector = Sectors.FromRules(domain);
        return sector is null
            ? new SectorAssignment(domain, Sectors.Other, SiteListFiles.NeedsReviewNote)
            : new SectorAssignment(domain, sector, "auto: domain rule");
    }
}
