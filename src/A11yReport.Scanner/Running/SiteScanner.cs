using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Scanner.Robots;
using A11yReport.Scanner.Scanning;

namespace A11yReport.Scanner.Running;

/// <summary>Scans one site: robots.txt first, then one page load.</summary>
public sealed class SiteScanner(RobotsChecker robots, PageScanner pages)
{
    public async Task<SiteScanResult> ScanAsync(SiteEntry site, int attempt, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var robotsCheck = await robots.CheckAsync(site.Url, cancellationToken);
        var robotsRequests = new RequestCounts(robotsCheck.Requests, 0, 0);
        if (robotsCheck.Stop is { } stop)
        {
            return Result(site, attempt, startedAt, stop, robotsCheck.Message, robotsRequests) with
            {
                HttpStatus = robotsCheck.HttpStatus,
            };
        }

        var outcome = await pages.ScanAsync(site.Url, cancellationToken);
        return Result(site, attempt, startedAt, outcome.Status, outcome.Message, robotsRequests.Add(outcome.Requests)) with
        {
            HttpStatus = outcome.HttpStatus,
            FinalUrl = outcome.Result?.FinalUrl,
            AxeVersion = outcome.Result?.AxeVersion,
            Facts = outcome.Result?.Facts,
            Viewports = outcome.Result?.Viewports,
            LoadEventReached = outcome.Result?.LoadEventReached,
        };
    }

    public static SiteScanResult Skipped(SiteEntry site, ScanStatus status, string message)
    {
        var now = DateTimeOffset.UtcNow;
        return Result(site, 0, now, status, message, RequestCounts.None);
    }

    private static SiteScanResult Result(
        SiteEntry site, int attempt, DateTimeOffset startedAt, ScanStatus status, string message, RequestCounts requests) =>
        new(site.Domain, site.Sector, site.Url, status, message, attempt, startedAt, DateTimeOffset.UtcNow, requests);
}
