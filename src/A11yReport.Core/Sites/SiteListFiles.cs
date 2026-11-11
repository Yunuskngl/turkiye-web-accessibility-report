using System.Globalization;
using A11yReport.Core.Csv;

namespace A11yReport.Core.Sites;

/// <summary>Reads and writes the hand-maintained files in data/.</summary>
public static class SiteListFiles
{
    private const string DomainColumn = "domain";
    private const string UrlColumn = "url";
    private const string BucketColumn = "rank_bucket";
    private const string SectorColumn = "sector";
    private const string NotesColumn = "notes";

    public const string NeedsReviewNote = "auto: needs review";

    public static IReadOnlySet<string> LoadDomains(string path)
    {
        if (!File.Exists(path))
        {
            return new HashSet<string>();
        }

        return CsvFile.Read(path)
            .Select(row => DomainKey.FromHost(row.GetValueOrDefault(DomainColumn, string.Empty)))
            .Where(domain => domain.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<SectorAssignment> LoadSectors(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        return CsvFile.Read(path)
            .Select(row => new SectorAssignment(
                row.GetValueOrDefault(DomainColumn, string.Empty),
                row.GetValueOrDefault(SectorColumn, string.Empty),
                row.GetValueOrDefault(NotesColumn, string.Empty)))
            .Where(assignment => assignment.Domain.Length > 0)
            .ToList();
    }

    public static void SaveSectors(string path, IEnumerable<SectorAssignment> assignments)
    {
        var rows = assignments.Select(a => (IReadOnlyList<string>)[a.Domain, a.Sector, a.Notes]);
        CsvFile.Write(path, [DomainColumn, SectorColumn, NotesColumn], rows);
    }

    public static IReadOnlyList<SiteEntry> LoadSites(string path)
    {
        return CsvFile.Read(path)
            .Select(row => new SiteEntry(
                row[DomainColumn],
                new Uri(row[UrlColumn]),
                int.Parse(row[BucketColumn], CultureInfo.InvariantCulture),
                row[SectorColumn]))
            .ToList();
    }

    public static void SaveSites(string path, IEnumerable<SiteEntry> sites)
    {
        var rows = sites.Select(site => (IReadOnlyList<string>)
            [site.Domain, site.Url.ToString(), site.RankBucket.ToString(CultureInfo.InvariantCulture), site.Sector]);
        CsvFile.Write(path, [DomainColumn, UrlColumn, BucketColumn, SectorColumn], rows);
    }
}
