using System.Text.Json;
using A11yReport.Core.Results;

namespace A11yReport.Storage;

internal static class ManifestBuilder
{
    public static RunManifest Build(ScanRunEntity run, List<SiteScanEntity> sites)
    {
        var manifestSites = sites.Select(ToManifestSite).ToList();
        var totals = manifestSites.Aggregate(RequestCounts.None, (sum, site) => sum.Add(site.Requests));
        var statusCounts = Enum.GetValues<ScanStatus>()
            .ToDictionary(status => status.ToString(), status => sites.Count(site => site.Status == status));

        return new RunManifest(
            run.Name, run.State.ToString(), run.ListSource, run.ListSha256,
            run.Sessions.OrderBy(s => s.Id).Select(ToSessionInfo).ToList(),
            statusCounts, totals, manifestSites);
    }

    private static ManifestSite ToManifestSite(SiteScanEntity site) => new(
        site.Domain, site.Sector, site.Status.ToString(), site.Attempts, site.HttpStatus,
        new RequestCounts(site.RobotsRequests, site.DocumentRequests, site.SubresourceRequests),
        site.AxeVersion);

    private static RunSessionInfo ToSessionInfo(RunSessionEntity session)
    {
        using var settings = JsonDocument.Parse(session.SettingsJson);
        return new RunSessionInfo(
            session.StartedAt, session.FinishedAt, session.State.ToString(),
            new ToolVersions(session.ScannerVersion, session.DotnetVersion, session.PlaywrightVersion,
                session.BrowserVersion, session.AxeVersion),
            session.UserAgent, settings.RootElement.Clone());
    }
}
