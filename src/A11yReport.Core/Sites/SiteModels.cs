namespace A11yReport.Core.Sites;

/// <summary>One origin from a CrUX snapshot. Rank is a bucket (1000, 5000, 10000, ...), not an exact rank.</summary>
public sealed record CruxOrigin(Uri Origin, int RankBucket);

/// <summary>One site to scan: one registrable domain and the home page URL we load.</summary>
public sealed record SiteCandidate(string Domain, Uri Url, int RankBucket);

/// <summary>
/// The chosen sites, plus domains left out because CrUX lists them only through subdomains
/// (for example an app or login portal) and not through a home page on the domain or www.
/// </summary>
public sealed record SiteListResult(IReadOnlyList<SiteCandidate> Sites, IReadOnlyList<string> SubdomainOnly);

public sealed record SiteEntry(string Domain, Uri Url, int RankBucket, string Sector)
{
    public bool IsPublicSector => Sector == Sectors.Public || Sectors.IsPublicZone(Domain);
}

public sealed record SectorAssignment(string Domain, string Sector, string Notes);
