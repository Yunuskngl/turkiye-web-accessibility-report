using A11yReport.Core.Report;
using A11yReport.Core.Results;
using Microsoft.EntityFrameworkCore;

namespace A11yReport.Storage;

/// <summary>Reads one run's results in the shape the report needs. Selectors and snippets are not read.</summary>
public sealed class ReportReader(IDbContextFactory<ReportDbContext> contexts)
{
    private const string Desktop = "Desktop";
    private const string Mobile = "Mobile";
    private const string Unknown = "unknown";

    /// <summary>Returns null when no run with this name exists.</summary>
    public async Task<(RunInfo Run, IReadOnlyList<SiteOutcome> Sites)?> ReadAsync(string runName, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var run = await db.Runs.AsNoTracking().Include(r => r.Sessions)
            .SingleOrDefaultAsync(r => r.Name == runName, cancellationToken);
        if (run is null)
        {
            return null;
        }

        var sites = await db.Sites.AsNoTracking().Include(s => s.Violations)
            .Where(s => s.RunId == run.Id).OrderBy(s => s.Domain).ToListAsync(cancellationToken);
        return (ToRunInfo(run), sites.Select(ToOutcome).ToList());
    }

    /// <summary>The axe rule ids that failed at least once in the run (no other data).</summary>
    public async Task<IReadOnlySet<string>> ReadRuleIdsAsync(string runName, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var ids = await db.Violations.AsNoTracking()
            .Where(v => db.Sites.Any(s => s.Id == v.SiteScanId && db.Runs.Any(r => r.Id == s.RunId && r.Name == runName)))
            .Select(v => v.RuleId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return ids.ToHashSet(StringComparer.Ordinal);
    }

    private static RunInfo ToRunInfo(ScanRunEntity run)
    {
        var sessions = run.Sessions.OrderBy(s => s.Id).ToList();
        var last = sessions[^1];
        var axe = sessions.Select(s => s.AxeVersion).LastOrDefault(v => v is not null) ?? Unknown;
        return new RunInfo(run.Name, run.State.ToString(), sessions[0].StartedAt, last.FinishedAt, axe,
            last.BrowserVersion, run.ListSource);
    }

    private static SiteOutcome ToOutcome(SiteScanEntity site) => new(
        site.Domain,
        site.Sector,
        site.Status,
        ToFacts(site),
        RulesFor(site, Desktop),
        RulesFor(site, Mobile),
        site.LoadEventReached);

    private static List<RuleCount> RulesFor(SiteScanEntity site, string viewport) =>
        site.Violations
            .Where(v => v.Viewport == viewport)
            .Select(v => new RuleCount(v.RuleId, v.Impact, v.Help, v.NodeCount)
            {
                Criteria = WcagCriteria.FromTags(v.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries)),
            })
            .ToList();

    private static PageFacts? ToFacts(SiteScanEntity site)
    {
        if (site.HasTitle is null)
        {
            return null;
        }

        return new PageFacts(site.Lang, site.HasTitle.Value, site.LandmarkCount ?? 0, site.HasMain ?? false,
            site.HeadingCount ?? 0, site.H1Count ?? 0, site.SkipsHeadingLevel ?? false);
    }
}
