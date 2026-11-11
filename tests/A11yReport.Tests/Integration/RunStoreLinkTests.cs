using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Storage;
using Microsoft.EntityFrameworkCore;

namespace A11yReport.Tests.Integration;

/// <summary>Checks that stored rows point at their parents through our own foreign key columns.</summary>
public sealed class RunStoreLinkTests : IAsyncLifetime, IDisposable
{
    private readonly TempStore _temp = new();

    public Task InitializeAsync() => _temp.Store.MigrateAsync(CancellationToken.None);

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose() => _temp.Dispose();

    [Fact]
    public async Task SaveSiteAsync_LinksViolationsAndNodesToTheirSite()
    {
        var handle = await _temp.Store.StartSessionAsync(Start(), CancellationToken.None);
        await _temp.Store.SaveSiteAsync(handle, DoneWithViolation("a.com.tr"), CancellationToken.None);

        await using var db = _temp.CreateContext();
        var site = await db.Sites.SingleAsync();
        var violation = await db.Violations.SingleAsync();
        Assert.Equal(site.Id, violation.SiteScanId);
        Assert.Equal(violation.Id, (await db.Set<ViolationNodeEntity>().SingleAsync()).ViolationId);
        Assert.Equal(handle.RunId, (await db.Sessions.SingleAsync()).RunId);
    }

    [Fact]
    public async Task SaveSiteAsync_Replace_DeletesOldViolations()
    {
        var handle = await _temp.Store.StartSessionAsync(Start(), CancellationToken.None);
        await _temp.Store.SaveSiteAsync(handle, DoneWithViolation("a.com.tr"), CancellationToken.None);
        await _temp.Store.SaveSiteAsync(handle, DoneWithViolation("a.com.tr"), CancellationToken.None);

        await using var db = _temp.CreateContext();
        Assert.Equal(1, await db.Violations.CountAsync());
        Assert.Equal(1, await db.Set<ViolationNodeEntity>().CountAsync());
    }

    private static RunStart Start() => new("links", "test", "sha", new ToolVersions("1", "10", "1.64", "156", null), "UA", "{}");

    private static SiteScanResult DoneWithViolation(string domain)
    {
        RuleViolation violation = new("image-alt", Impact.Critical, "Images must have alternative text", ["wcag2a", "wcag111"],
            [new ViolationNode("img", "<img src=\"a.png\">")]);
        return new SiteScanResult(domain, Sectors.Other, new Uri($"https://www.{domain}/"), ScanStatus.Done, "Scanned.", 1,
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, RequestCounts.None,
            Viewports: [new ViewportResult("Desktop", 1280, 800, [violation])]);
    }
}
