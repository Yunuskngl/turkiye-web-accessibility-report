using A11yReport.Core.Results;
using A11yReport.Scanner.Robots;
using A11yReport.Scanner.Scanning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace A11yReport.Scanner.Commands;

/// <summary>Scans one URL (robots.txt check + one page load) and prints a readable summary.</summary>
public sealed class UrlCommand(IServiceProvider services, RobotsChecker robots, ILogger<UrlCommand> logger)
{
    public async Task<int> RunAsync(Uri url, CancellationToken cancellationToken)
    {
        var robotsCheck = await robots.CheckAsync(url, cancellationToken);
        if (robotsCheck.Stop is { } stop)
        {
            await Console.Error.WriteLineAsync($"Not scanned ({stop}): {robotsCheck.Message}");
            return ExitCodes.ScanFailed;
        }

        await using var session = await BrowserStarter.TryStartAsync(logger);
        if (session is null)
        {
            return ExitCodes.ScanFailed;
        }

        var scanner = ActivatorUtilities.CreateInstance<PageScanner>(services, session.Browser);
        var outcome = await scanner.ScanAsync(url, cancellationToken);
        return await PrintAsync(outcome, robotsCheck.Requests);
    }

    private static async Task<int> PrintAsync(PageScanOutcome outcome, int robotsRequests)
    {
        if (outcome.Result is null)
        {
            await Console.Error.WriteLineAsync($"Scan failed ({outcome.Status}): {outcome.Message}");
            return ExitCodes.ScanFailed;
        }

        var requests = outcome.Requests.Add(new RequestCounts(robotsRequests, 0, 0));
        Console.Write(SummaryFormatter.Build(outcome.Result));
        Console.WriteLine(
            $"Requests:   {requests.Total} (robots.txt {requests.Robots}, page {requests.Documents}, " +
            $"resources loaded by the browser {requests.Subresources})");
        return ExitCodes.Success;
    }
}
