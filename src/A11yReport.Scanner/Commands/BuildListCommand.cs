using System.Globalization;
using A11yReport.Core;
using A11yReport.Core.Csv;
using A11yReport.Core.Sites;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner.Commands;

/// <summary>Builds data/sites.csv from a CrUX snapshot, the include list, the opt-out list and data/sectors.csv.</summary>
public sealed class BuildListCommand(IOptions<DataOptions> options)
{
    private const string SourceName =
        "Chrome UX Report (CrUX) top origins for Türkiye (country code tr), via github.com/zakird/crux-top-lists";

    private readonly DataOptions _data = options.Value;

    public int Run(string snapshotPath)
    {
        if (!File.Exists(snapshotPath))
        {
            Console.Error.WriteLine($"Snapshot file not found: {snapshotPath}");
            return ExitCodes.Usage;
        }

        var origins = CruxReader.Read(snapshotPath);
        var include = SiteListFiles.LoadDomains(_data.IncludeFile);
        var excluded = SiteListFiles.LoadDomains(_data.OptOutFile).Union(SiteListFiles.LoadDomains(_data.ExcludeFile)).ToHashSet();
        var candidates = SiteListBuilder.Build(origins, include, excluded, _data.MaxSites);

        var sectors = SectorAssigner.AddMissing(SiteListFiles.LoadSectors(_data.SectorsFile), candidates.Select(c => c.Domain));
        SiteListFiles.SaveSectors(_data.SectorsFile, sectors);

        var sites = candidates
            .Select(c => new SiteEntry(c.Domain, c.Url, c.RankBucket, SectorAssigner.SectorOf(c.Domain, sectors)))
            .ToList();
        SiteListFiles.SaveSites(_data.SitesFile, sites);
        SaveSource(snapshotPath, sites.Count);
        SaveOtherTldCandidates(origins);
        PrintSummary(sites, sectors);
        return ExitCodes.Success;
    }

    private void SaveSource(string snapshotPath, int count)
    {
        var source = new SiteListSource(SourceName, Path.GetFileName(snapshotPath), FileHash.Sha256(snapshotPath),
            DateTimeOffset.UtcNow, count, _data.MaxSites);
        source.Save(_data.SitesSourceFile);
    }

    private void SaveOtherTldCandidates(IReadOnlyList<CruxOrigin> origins)
    {
        var candidates = SiteListBuilder.OtherTldCandidates(origins, _data.OtherTldCandidateMaxBucket);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_data.OtherTldCandidatesFile))!);
        CsvFile.Write(_data.OtherTldCandidatesFile, ["domain", "rank_bucket"],
            candidates.Select(c => (IReadOnlyList<string>)[c.Domain, c.RankBucket.ToString(CultureInfo.InvariantCulture)]));
    }

    private void PrintSummary(List<SiteEntry> sites, IReadOnlyList<SectorAssignment> sectors)
    {
        Console.WriteLine($"Wrote {sites.Count} sites to {_data.SitesFile} (limit {_data.MaxSites}).");
        foreach (var bucket in sites.GroupBy(s => s.RankBucket).OrderBy(g => g.Key))
        {
            Console.WriteLine($"  rank bucket {bucket.Key,7}: {bucket.Count(),4} sites");
        }

        foreach (var sector in sites.GroupBy(s => s.Sector).OrderByDescending(g => g.Count()))
        {
            Console.WriteLine($"  sector {sector.Key,-11}: {sector.Count(),4} sites");
        }

        var listed = sites.Select(s => s.Domain).ToHashSet();
        var toReview = sectors.Count(a => listed.Contains(a.Domain) && a.Notes == SiteListFiles.NeedsReviewNote);
        Console.WriteLine($"{toReview} listed domains in {_data.SectorsFile} still need a sector review.");
        Console.WriteLine($"Non-.tr candidates for {_data.IncludeFile} written to {_data.OtherTldCandidatesFile}.");
    }
}
