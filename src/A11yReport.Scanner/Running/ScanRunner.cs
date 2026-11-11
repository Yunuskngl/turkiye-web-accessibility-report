using System.Reflection;
using System.Text.Json;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Running;

/// <summary>
/// Runs a whole scan: skips sites already done in this run, scans the rest with a concurrency
/// limit and a global delay between site starts, gives transient failures one retry, and
/// always writes the run manifest at the end (also after Ctrl+C).
/// </summary>
public sealed class ScanRunner(
    IBrowser browser,
    SiteScanner siteScanner,
    RunStore store,
    IOptions<ScanOptions> scanOptions,
    IOptions<RunOptions> runOptions,
    IOptions<StorageOptions> storageOptions,
    ILogger<ScanRunner> logger)
{
    private const int FirstAttempt = 1;
    private const int RetryAttempt = 2;
    private readonly RunOptions _run = runOptions.Value;
    private string? _axeVersion;

    public async Task<RunManifest> RunAsync(RunPlan plan, CancellationToken cancellationToken)
    {
        await store.MigrateAsync(cancellationToken);
        var handle = await store.StartSessionAsync(BuildRunStart(plan), cancellationToken);
        var state = RunState.Interrupted;
        try
        {
            await ScanPendingAsync(plan, handle, cancellationToken);
            await RetryTransientAsync(plan, handle, cancellationToken);
            state = RunState.Completed;
        }
        finally
        {
            // Runs after Ctrl+C too, so an interrupted run still leaves its audit record.
            await store.FinishSessionAsync(handle, state, _axeVersion);
            await WriteManifestAsync(handle);
        }

        return await store.BuildManifestAsync(handle.RunId);
    }

    private async Task WriteManifestAsync(RunHandle handle)
    {
        var manifest = await store.BuildManifestAsync(handle.RunId);
        var path = await ManifestWriter.WriteAsync(manifest, _run.ManifestFolder);
        Log.ManifestWritten(logger, path);
    }

    private async Task ScanPendingAsync(RunPlan plan, RunHandle handle, CancellationToken cancellationToken)
    {
        var done = await store.GetScannedDomainsAsync(handle.RunId, cancellationToken);
        var pending = plan.Sites.Where(site => !done.Contains(site.Domain)).ToList();
        Log.RunStarted(logger, plan.Name, plan.Sites.Count, pending.Count, handle.IsResume);
        await ScanAllAsync(pending, plan.OptOut, handle, FirstAttempt, cancellationToken);
    }

    private async Task RetryTransientAsync(RunPlan plan, RunHandle handle, CancellationToken cancellationToken)
    {
        var retry = await store.GetRetryDomainsAsync(handle.RunId, cancellationToken);
        var sites = plan.Sites.Where(site => retry.Contains(site.Domain)).ToList();
        if (sites.Count == 0)
        {
            return;
        }

        Log.RetryStarted(logger, sites.Count);
        await ScanAllAsync(sites, plan.OptOut, handle, RetryAttempt, cancellationToken);
    }

    private async Task ScanAllAsync(
        List<SiteEntry> sites, IReadOnlySet<string> optOut, RunHandle handle, int attempt, CancellationToken cancellationToken)
    {
        using var gate = new StartGate(TimeSpan.FromSeconds(_run.DelayBetweenSitesSeconds), TimeProvider.System);
        var finished = 0;
        var parallel = new ParallelOptions { MaxDegreeOfParallelism = _run.Concurrency, CancellationToken = cancellationToken };
        await Parallel.ForEachAsync(sites, parallel, async (site, token) =>
        {
            var result = await ScanOneAsync(site, optOut, attempt, gate, token);
            var stored = await store.SaveSiteAsync(handle, result, token);
            _axeVersion ??= result.AxeVersion;
            var count = Interlocked.Increment(ref finished);
            Log.SiteFinished(logger, count, sites.Count, site.Domain, stored);
        });
    }

    private async Task<SiteScanResult> ScanOneAsync(
        SiteEntry site, IReadOnlySet<string> optOut, int attempt, StartGate gate, CancellationToken cancellationToken)
    {
        if (optOut.Contains(site.Domain))
        {
            return SiteScanner.Skipped(site, ScanStatus.SkippedOptOut, "The site owner asked not to be scanned.");
        }

        await gate.WaitAsync(cancellationToken);
        return await siteScanner.ScanAsync(site, attempt, cancellationToken);
    }

    private RunStart BuildRunStart(RunPlan plan)
    {
        var scan = scanOptions.Value;
        var settings = JsonSerializer.Serialize(new
        {
            Scan = scan,
            Run = _run,
            storageOptions.Value.MaxNodesPerRule,
        });
        return new RunStart(plan.Name, plan.ListSource, plan.ListSha256, Versions(), scan.UserAgent, settings);
    }

    private ToolVersions Versions() => new(
        VersionOf(typeof(ScanRunner).Assembly),
        Environment.Version.ToString(),
        FileVersionOf(typeof(IPlaywright).Assembly),
        browser.Version,
        null);

    private static string VersionOf(Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? assembly.GetName().Version?.ToString()
        ?? "unknown";

    // Playwright's informational version is a placeholder ("1.0.0+commit"); its file version is the package version.
    private static string FileVersionOf(Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? VersionOf(assembly);
}
