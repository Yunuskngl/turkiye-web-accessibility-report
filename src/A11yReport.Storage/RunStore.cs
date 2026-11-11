using System.Text.Json;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace A11yReport.Storage;

/// <summary>
/// All database access for scan runs. Writes are serialized because SQLite allows one writer
/// and the scanner saves results from several parallel tasks.
/// </summary>
public sealed class RunStore(IDbContextFactory<ReportDbContext> contexts, IOptions<StorageOptions> options) : IDisposable
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly StorageOptions _options = options.Value;

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }

    public async Task<RunHandle> StartSessionAsync(RunStart start, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var run = await db.Runs.SingleOrDefaultAsync(r => r.Name == start.Name, cancellationToken);
        EnsureCanResume(run, start);

        var isResume = run is not null;
        run ??= db.Runs.Add(NewRun(start)).Entity;
        run.State = RunState.Running;
        var session = NewSession(start);
        run.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return new RunHandle(run.Id, session.Id, isResume);
    }

    public async Task<IReadOnlySet<string>> GetScannedDomainsAsync(int runId, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var domains = await db.Sites.Where(s => s.RunId == runId).Select(s => s.Domain).ToListAsync(cancellationToken);
        return domains.ToHashSet();
    }

    /// <summary>Sites that failed once with a transient error and may get their single retry.</summary>
    public async Task<IReadOnlySet<string>> GetRetryDomainsAsync(int runId, CancellationToken cancellationToken)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var rows = await db.Sites
            .Where(s => s.RunId == runId && s.Attempts == 1)
            .Select(s => new { s.Domain, s.Sector, s.Status })
            .ToListAsync(cancellationToken);
        return rows
            .Where(row => ScanStatusRules.IsRetryable(row.Status))
            .Where(row => row.Sector != Sectors.Public && !Sectors.IsPublicZone(row.Domain))
            .Select(row => row.Domain)
            .ToHashSet();
    }

    /// <summary>Saves (or replaces) one site's result and returns the status that was stored.</summary>
    public async Task<ScanStatus> SaveSiteAsync(RunHandle handle, SiteScanResult result, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await using var db = await contexts.CreateDbContextAsync(cancellationToken);
            var entity = SiteScanMapper.ToEntity(result, handle.RunId, handle.SessionId, _options.MaxNodesPerRule);
            await MarkDuplicateAsync(db, entity, cancellationToken);
            await db.Sites.Where(s => s.RunId == handle.RunId && s.Domain == entity.Domain).ExecuteDeleteAsync(cancellationToken);
            db.Sites.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            return entity.Status;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task FinishSessionAsync(RunHandle handle, RunState state, string? axeVersion)
    {
        await using var db = await contexts.CreateDbContextAsync();
        var session = await db.Sessions.SingleAsync(s => s.Id == handle.SessionId);
        session.FinishedAt = DateTimeOffset.UtcNow;
        session.State = state;
        session.AxeVersion = axeVersion;
        var run = await db.Runs.SingleAsync(r => r.Id == handle.RunId);
        run.State = state;
        await db.SaveChangesAsync();
    }

    public async Task<RunManifest> BuildManifestAsync(int runId)
    {
        await using var db = await contexts.CreateDbContextAsync();
        var run = await db.Runs.AsNoTracking().Include(r => r.Sessions).SingleAsync(r => r.Id == runId);
        var sites = await db.Sites.AsNoTracking().Where(s => s.RunId == runId).OrderBy(s => s.Domain).ToListAsync();
        return ManifestBuilder.Build(run, sites);
    }

    public void Dispose() => _writeLock.Dispose();

    private static void EnsureCanResume(ScanRunEntity? run, RunStart start)
    {
        if (run is null)
        {
            return;
        }

        if (run.State == RunState.Completed)
        {
            throw new RunStartException($"Run '{start.Name}' is already completed. Use a new run name.");
        }

        if (run.ListSha256 != start.ListSha256)
        {
            throw new RunStartException($"Run '{start.Name}' was started with a different site list. Use a new run name.");
        }
    }

    private static async Task MarkDuplicateAsync(ReportDbContext db, SiteScanEntity entity, CancellationToken cancellationToken)
    {
        if (entity.Status != ScanStatus.Done || entity.FinalSiteKey is null)
        {
            return;
        }

        var taken = await db.Sites.AnyAsync(s => s.RunId == entity.RunId && s.Domain != entity.Domain
            && s.Status == ScanStatus.Done && s.FinalSiteKey == entity.FinalSiteKey, cancellationToken);
        if (!taken)
        {
            return;
        }

        entity.Status = ScanStatus.Duplicate;
        entity.Message = $"Redirects to {entity.FinalSiteKey}, which is already in this run.";
        entity.Violations = [];
    }

    private static ScanRunEntity NewRun(RunStart start) => new()
    {
        Name = start.Name,
        ListSource = start.ListSource,
        ListSha256 = start.ListSha256,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static RunSessionEntity NewSession(RunStart start) => new()
    {
        StartedAt = DateTimeOffset.UtcNow,
        State = RunState.Running,
        ScannerVersion = start.Versions.Scanner,
        DotnetVersion = start.Versions.Dotnet,
        PlaywrightVersion = start.Versions.Playwright,
        BrowserVersion = start.Versions.Browser,
        UserAgent = start.UserAgent,
        SettingsJson = start.SettingsJson,
    };
}
