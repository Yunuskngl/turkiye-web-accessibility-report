namespace A11yReport.Core.Sites;

/// <summary>One origin from a CrUX snapshot. Rank is a bucket (1000, 5000, 10000, ...), not an exact rank.</summary>
public sealed record CruxOrigin(Uri Origin, int RankBucket);

/// <summary>One site to scan: one registrable domain and the home page URL we load.</summary>
public sealed record SiteCandidate(string Domain, Uri Url, int RankBucket);

public sealed record SiteEntry(string Domain, Uri Url, int RankBucket, string Sector)
{
    public bool IsPublicSector => Sector == Sectors.Public || Sectors.IsPublicZone(Domain);
}

public sealed record SectorAssignment(string Domain, string Sector, string Notes);
