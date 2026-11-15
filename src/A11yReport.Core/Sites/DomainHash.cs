using System.Security.Cryptography;
using System.Text;

namespace A11yReport.Core.Sites;

/// <summary>
/// SHA-256 of a domain key, as lowercase hex. Used for lookup file names and for the hidden-domain
/// list, so that domains we must not publish (for example a person's name) never appear in the repository.
/// </summary>
public static class DomainHash
{
    public static string Of(string domain) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(domain.Trim().ToLowerInvariant())));

    /// <summary>The domains from <paramref name="domains"/> whose hash is in <paramref name="hiddenHashes"/>.</summary>
    public static IReadOnlySet<string> Hidden(IEnumerable<string> domains, IReadOnlySet<string> hiddenHashes) =>
        domains.Where(domain => hiddenHashes.Contains(Of(domain))).ToHashSet(StringComparer.OrdinalIgnoreCase);
}
