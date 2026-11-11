using System.Security.Cryptography;
using System.Text;

namespace A11yReport.Core.Sites;

public static class SiteListBuilder
{
    /// <summary>
    /// Picks Turkish sites from CrUX origins: one entry per registrable domain, best rank bucket first.
    /// CrUX does not order sites inside a bucket, so ties are broken by a hash of the domain:
    /// stable between runs and not biased toward any letter of the alphabet.
    /// </summary>
    public static IReadOnlyList<SiteCandidate> Build(
        IEnumerable<CruxOrigin> origins, IReadOnlySet<string> includeDomains, IReadOnlySet<string> optOut, int maxSites)
    {
        return origins
            .Where(origin => IsWebOrigin(origin.Origin))
            .GroupBy(origin => DomainKey.FromHost(origin.Origin.Host))
            .Where(group => DomainKey.IsTurkishTld(group.Key) || includeDomains.Contains(group.Key))
            .Where(group => !optOut.Contains(group.Key))
            .Select(group => ToCandidate(group.Key, group.ToList()))
            .OrderBy(candidate => candidate.RankBucket)
            .ThenBy(candidate => StableHash(candidate.Domain), StringComparer.Ordinal)
            .Take(maxSites)
            .ToList();
    }

    /// <summary>Non-Turkish-TLD domains in the best buckets, for a person to review for the include list.</summary>
    public static IReadOnlyList<(string Domain, int RankBucket)> OtherTldCandidates(
        IEnumerable<CruxOrigin> origins, int maxBucket)
    {
        return origins
            .Where(origin => origin.RankBucket <= maxBucket && IsWebOrigin(origin.Origin))
            .GroupBy(origin => DomainKey.FromHost(origin.Origin.Host))
            .Where(group => !DomainKey.IsTurkishTld(group.Key))
            .Select(group => (group.Key, group.Min(origin => origin.RankBucket)))
            .OrderBy(item => item.Item2)
            .ThenBy(item => item.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static SiteCandidate ToCandidate(string domain, List<CruxOrigin> origins)
    {
        var bestBucket = origins.Min(origin => origin.RankBucket);
        var home = origins
            .Where(origin => DomainKey.IsHomeHostOf(origin.Origin.Host, domain))
            .OrderBy(origin => origin.RankBucket)
            .ThenByDescending(origin => origin.Origin.Scheme == Uri.UriSchemeHttps)
            .FirstOrDefault();

        // When CrUX only lists subdomains (for example a student portal), we load the www home page instead.
        var url = home is null ? new Uri($"https://www.{domain}/") : new Uri(home.Origin, "/");
        return new SiteCandidate(domain, url, bestBucket);
    }

    private static bool IsWebOrigin(Uri origin) =>
        (origin.Scheme == Uri.UriSchemeHttps || origin.Scheme == Uri.UriSchemeHttp) && origin.IsDefaultPort;

    private static string StableHash(string domain) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(domain)));
}
