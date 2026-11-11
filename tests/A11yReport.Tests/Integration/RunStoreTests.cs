using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Storage;

namespace A11yReport.Tests.Integration;

public sealed class RunStoreTests : IAsyncLifetime, IDisposable
{
    private static readonly ToolVersions Versions = new("1.0", "10.0", "1.64", "156", null);
    private readonly TempStore _temp = new();
    private RunStore Store => _temp.Store;

    public Task InitializeAsync() => Store.MigrateAsync(CancellationToken.None);

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose() => _temp.Dispose();

    [Fact]
    public async Task StartSessionAsync_SameName_ResumesAndKeepsScannedSites()
    {
        var first = await Store.StartSessionAsync(Start("pilot"), CancellationToken.None);
        await Store.SaveSiteAsync(first, Result("a.com.tr", ScanStatus.Done), CancellationToken.None);
        await Store.FinishSessionAsync(first, RunState.Interrupted, null);

        var second = await Store.StartSessionAsync(Start("pilot"), CancellationToken.None);

        Assert.True(second.IsResume);
        Assert.Equal(first.RunId, second.RunId);
        Assert.Contains("a.com.tr", await Store.GetScannedDomainsAsync(second.RunId, CancellationToken.None));
    }

    [Fact]
    public async Task StartSessionAsync_CompletedRun_IsRefused()
    {
        var handle = await Store.StartSessionAsync(Start("done"), CancellationToken.None);
        await Store.FinishSessionAsync(handle, RunState.Completed, "4.14.0");

        await Assert.ThrowsAsync<RunStartException>(() => Store.StartSessionAsync(Start("done"), CancellationToken.None));
    }

    [Fact]
    public async Task StartSessionAsync_DifferentList_IsRefused()
    {
        await Store.StartSessionAsync(Start("pilot", listSha: "aaa"), CancellationToken.None);

        await Assert.ThrowsAsync<RunStartException>(() =>
            Store.StartSessionAsync(Start("pilot", listSha: "bbb"), CancellationToken.None));
    }

    [Fact]
    public async Task SaveSiteAsync_SecondSiteWithSameFinalSite_IsDuplicate()
    {
        var handle = await Store.StartSessionAsync(Start("dup"), CancellationToken.None);
        var target = new Uri("https://www.example.com.tr/");
        await Store.SaveSiteAsync(handle, Result("example.com.tr", ScanStatus.Done) with { FinalUrl = target }, CancellationToken.None);

        var stored = await Store.SaveSiteAsync(handle, Result("example-alias.com.tr", ScanStatus.Done) with { FinalUrl = target },
            CancellationToken.None);

        Assert.Equal(ScanStatus.Duplicate, stored);
    }

    [Fact]
    public async Task GetRetryDomainsAsync_SkipsPublicSectorAndBlockedSites()
    {
        var handle = await Store.StartSessionAsync(Start("retry"), CancellationToken.None);
        await Store.SaveSiteAsync(handle, Result("shop.com.tr", ScanStatus.Timeout), CancellationToken.None);
        await Store.SaveSiteAsync(handle, Result("kurum.gov.tr", ScanStatus.Timeout, Sectors.Public), CancellationToken.None);
        await Store.SaveSiteAsync(handle, Result("blocked.com.tr", ScanStatus.Unreachable), CancellationToken.None);

        var retry = await Store.GetRetryDomainsAsync(handle.RunId, CancellationToken.None);

        Assert.Equal(["shop.com.tr"], retry);
    }

    [Fact]
    public async Task BuildManifestAsync_CountsStatusesAndRequests()
    {
        var handle = await Store.StartSessionAsync(Start("manifest"), CancellationToken.None);
        await Store.SaveSiteAsync(handle, Result("a.com.tr", ScanStatus.Done) with { Requests = new(1, 1, 10) }, CancellationToken.None);
        await Store.SaveSiteAsync(handle, Result("b.com.tr", ScanStatus.SkippedRobots) with { Requests = new(1, 0, 0) },
            CancellationToken.None);
        await Store.FinishSessionAsync(handle, RunState.Completed, "4.14.0");

        var manifest = await Store.BuildManifestAsync(handle.RunId);

        Assert.Equal(1, manifest.StatusCounts["Done"]);
        Assert.Equal(1, manifest.StatusCounts["SkippedRobots"]);
        Assert.Equal(new RequestCounts(2, 1, 10), manifest.RequestTotals);
        Assert.Equal("4.14.0", manifest.Sessions.Single().Versions.AxeCore);
    }

    private static RunStart Start(string name, string listSha = "sha") =>
        new(name, "test list", listSha, Versions, "UA", "{}");

    private static SiteScanResult Result(string domain, ScanStatus status, string sector = Sectors.Other) =>
        new(domain, sector, new Uri($"https://www.{domain}/"), status, "test", 1,
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch, RequestCounts.None);
}
