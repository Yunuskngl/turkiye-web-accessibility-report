using A11yReport.Core;
using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Scanner.Running;
using A11yReport.Scanner.Scanning;
using A11yReport.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner.Commands;

/// <summary>Runs (or resumes) a named scan over data/sites.csv.</summary>
public sealed class ScanRunCommand(
    IServiceProvider services,
    IOptions<ScanOptions> scanOptions,
    IOptions<DataOptions> dataOptions,
    ILogger<ScanRunCommand> logger)
{
    private readonly DataOptions _data = dataOptions.Value;

    public async Task<int> RunAsync(ScanArgs args, CancellationToken cancellationToken)
    {
        var problem = FindSetupProblem();
        if (problem is not null)
        {
            await Console.Error.WriteLineAsync(problem);
            return ExitCodes.Usage;
        }

        await using var session = await BrowserStarter.TryStartAsync(logger);
        if (session is null)
        {
            return ExitCodes.ScanFailed;
        }

        return await RunWithBrowserAsync(session, BuildPlan(args), cancellationToken);
    }

    private async Task<int> RunWithBrowserAsync(BrowserSession session, RunPlan plan, CancellationToken cancellationToken)
    {
        var pages = ActivatorUtilities.CreateInstance<PageScanner>(services, session.Browser);
        var sites = ActivatorUtilities.CreateInstance<SiteScanner>(services, pages);
        var runner = ActivatorUtilities.CreateInstance<ScanRunner>(services, session.Browser, sites);
        try
        {
            PrintSummary(await runner.RunAsync(plan, cancellationToken));
            return ExitCodes.Success;
        }
        catch (RunStartException exception)
        {
            await Console.Error.WriteLineAsync(exception.Message);
            return ExitCodes.Usage;
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Scan stopped. Run the same command again to resume.");
            return ExitCodes.Cancelled;
        }
    }

    private string? FindSetupProblem()
    {
        if (string.IsNullOrWhiteSpace(scanOptions.Value.MethodologyUrl))
        {
            return "Set Scan:MethodologyUrl in appsettings.json first: the User-Agent must link to the methodology page.";
        }

        var missing = new[] { _data.SitesFile, _data.SitesSourceFile }.FirstOrDefault(path => !File.Exists(path));
        return missing is null ? null : $"{missing} not found. Run build-list first.";
    }

    private RunPlan BuildPlan(ScanArgs args)
    {
        var sites = SiteListFiles.LoadSites(_data.SitesFile);
        var selected = args.Limit is { } limit ? sites.Take(limit).ToList() : sites;
        var source = SiteListSource.Load(_data.SitesSourceFile);
        return new RunPlan(args.RunName, selected, SiteListFiles.LoadDomains(_data.OptOutFile),
            source.Describe(), FileHash.Sha256(_data.SitesFile));
    }

    private static void PrintSummary(RunManifest manifest)
    {
        Console.WriteLine($"Run {manifest.Name}: {manifest.State}");
        foreach (var (status, count) in manifest.StatusCounts.Where(pair => pair.Value > 0))
        {
            Console.WriteLine($"  {status,-14} {count,5}");
        }

        var totals = manifest.RequestTotals;
        Console.WriteLine($"Requests: {totals.Total} (robots.txt {totals.Robots}, pages {totals.Documents}, " +
            $"resources loaded by the browser {totals.Subresources})");
    }
}
