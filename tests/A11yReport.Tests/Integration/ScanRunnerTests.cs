using A11yReport.Core.Results;
using A11yReport.Core.Sites;
using A11yReport.Scanner.Robots;
using A11yReport.Scanner.Running;
using A11yReport.Scanner.Scanning;
using A11yReport.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace A11yReport.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class ScanRunnerTests(ScannerFixture fixture) : IClassFixture<ScannerFixture>, IAsyncLifetime, IDisposable
{
    private const string AllowAll = "User-agent: *\nDisallow: /private/\n";
    private const string DisallowUs = "User-agent: " + TestSettings.RobotsToken + "\nDisallow: /\n\nUser-agent: *\nAllow: /\n";

    private readonly TempStore _temp = new();
    private readonly FixtureServer _allowed = new(AllowAll);
    private readonly FixtureServer _disallowed = new(DisallowUs);
    private readonly FixtureServer _optedOut = new(AllowAll);

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        _allowed.Dispose();
        _disallowed.Dispose();
        _optedOut.Dispose();
        _temp.Dispose();
    }

    [Fact]
    public async Task RunAsync_RecordsEveryOutcomeAndRespectsRobotsAndOptOut()
    {
        var manifest = await CreateRunner().RunAsync(Plan("test-run"), CancellationToken.None);

        var status = manifest.Sites.ToDictionary(site => site.Domain, site => site.Status);
        Assert.Equal("Done", status["allowed.test"]);
        Assert.Equal("SkippedRobots", status["disallowed.test"]);
        Assert.Equal("SkippedOptOut", status["opted-out.test"]);
        Assert.Equal("Unreachable", status["closed.test"]);
        Assert.Equal("Completed", manifest.State);
    }

    [Fact]
    public async Task RunAsync_NeverLoadsPagesThatAreDisallowedOrOptedOut()
    {
        await CreateRunner().RunAsync(Plan("no-load"), CancellationToken.None);

        Assert.Equal(["/robots.txt"], _disallowed.RequestedPaths);
        Assert.Empty(_optedOut.RequestedPaths);
        Assert.Equal(["/robots.txt", "/"], _allowed.RequestedPaths);
    }

    [Fact]
    public async Task RunAsync_WritesManifestFileWithRequestCounts()
    {
        var manifest = await CreateRunner().RunAsync(Plan("with-manifest"), CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(_temp.Folder, "runs", "with-manifest.json")));
        var allowed = manifest.Sites.Single(site => site.Domain == "allowed.test");
        Assert.Equal(1, allowed.Requests.Robots);
        Assert.Equal(1, allowed.Requests.Documents);
    }

    [Fact]
    public async Task RunAsync_CompletedRun_CannotRunAgain()
    {
        var runner = CreateRunner();
        await runner.RunAsync(Plan("once"), CancellationToken.None);

        await Assert.ThrowsAsync<RunStartException>(() => runner.RunAsync(Plan("once"), CancellationToken.None));
    }

    private RunPlan Plan(string name)
    {
        SiteEntry[] sites =
        [
            new("allowed.test", _allowed.BaseUrl, 1000, Sectors.Other),
            new("disallowed.test", _disallowed.BaseUrl, 1000, Sectors.Other),
            new("opted-out.test", _optedOut.BaseUrl, 1000, Sectors.Other),
            new("closed.test", FixtureServer.ClosedPortUrl(), 1000, Sectors.Other),
        ];
        return new RunPlan(name, sites, new HashSet<string> { "opted-out.test" }, "test list", "sha");
    }

    private ScanRunner CreateRunner()
    {
        var scan = Options.Create(TestSettings.Scan());
        var run = Options.Create(TestSettings.Run(Path.Combine(_temp.Folder, "runs")));
        var robots = new RobotsChecker(scan, run, NullLogger<RobotsChecker>.Instance);
        var pages = new PageScanner(fixture.Session.Browser, scan, NullLogger<PageScanner>.Instance);
        return new ScanRunner(fixture.Session.Browser, new SiteScanner(robots, pages), _temp.Store, scan, run,
            Options.Create(TestSettings.Storage("unused")), NullLogger<ScanRunner>.Instance);
    }
}
