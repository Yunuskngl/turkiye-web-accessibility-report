using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace A11yReport.Site.Lookup;

/// <summary>
/// Writes one JSON file per site, named by the SHA-256 of its domain. The site publishes no list of
/// domains: a result is found only by typing the domain, and the page computes the same hash.
/// </summary>
public static class LookupFiles
{
    public const string FolderName = "sites";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static string FileNameFor(string domain) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(domain))) + ".json";

    /// <summary>Replaces the whole folder, so results of sites that opted out since the last run disappear.</summary>
    public static void WriteAll(string siteFolder, IEnumerable<LookupSite> sites)
    {
        var folder = Path.Combine(siteFolder, FolderName);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }

        Directory.CreateDirectory(folder);
        foreach (var site in sites)
        {
            File.WriteAllText(Path.Combine(folder, FileNameFor(site.Domain)), JsonSerializer.Serialize(site, JsonOptions));
        }
    }
}
