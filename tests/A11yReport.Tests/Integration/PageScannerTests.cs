using A11yReport.Core.Results;
using A11yReport.Scanner.Scanning;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace A11yReport.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class PageScannerTests(ScannerFixture fixture) : IClassFixture<ScannerFixture>
{
    private const string KnownFailuresPage = "known-failures.html";

    private static readonly string[] ExpectedRuleIds =
    [
        "html-has-lang", "document-title", "image-alt", "link-name", "button-name", "label", "color-contrast",
    ];

    [Fact]
    public async Task ScanAsync_FindsKnownFailures_InBothViewports()
    {
        var outcome = await ScanAsync(KnownFailuresPage);

        Assert.Equal(ScanStatus.Done, outcome.Status);
        Assert.Equal(2, outcome.Result!.Viewports.Count);
        foreach (var viewport in outcome.Result.Viewports)
        {
            var found = viewport.Violations.Select(violation => violation.RuleId).ToHashSet();
            Assert.Superset(ExpectedRuleIds.ToHashSet(), found);
        }
    }

    [Fact]
    public async Task ScanAsync_RecordsAxeVersionHttpStatusAndOnePageLoad()
    {
        var outcome = await ScanAsync(KnownFailuresPage);

        Assert.Matches(@"^\d+\.\d+\.\d+", outcome.Result!.AxeVersion);
        Assert.Equal(200, outcome.HttpStatus);
        Assert.Equal(1, outcome.Requests.Documents);
        Assert.True(outcome.Result.LoadEventReached);
    }

    [Fact]
    public async Task ScanAsync_PageThatNeverFinishesLoading_IsScannedAnyway()
    {
        var options = TestSettings.Scan();
        options.LoadWaitSeconds = 1;
        var scanner = new PageScanner(fixture.Session.Browser, Options.Create(options), NullLogger<PageScanner>.Instance);

        var outcome = await scanner.ScanAsync(fixture.Server.UrlFor("slow-load.html"), CancellationToken.None);

        Assert.Equal(ScanStatus.Done, outcome.Status);
        Assert.False(outcome.Result!.LoadEventReached);
    }

    [Fact]
    public async Task ScanAsync_ReadsPageFacts()
    {
        var facts = (await ScanAsync(KnownFailuresPage)).Result!.Facts;

        Assert.Null(facts.Lang);
        Assert.False(facts.HasTitle);
        Assert.True(facts.HasMain);
        Assert.Equal(1, facts.LandmarkCount);
        Assert.Equal(1, facts.H1Count);
        Assert.False(facts.SkipsHeadingLevel);
    }

    [Fact]
    public async Task ScanAsync_KeepsSnippetsWithinLimit()
    {
        var outcome = await ScanAsync(KnownFailuresPage);

        var snippets = outcome.Result!.Viewports.SelectMany(v => v.Violations).SelectMany(v => v.Nodes).Select(n => n.Snippet);
        Assert.All(snippets, snippet => Assert.True(snippet.Length <= Snippet.MaxLength));
    }

    [Fact]
    public async Task ScanAsync_MissingPage_ReturnsHttpError()
    {
        var outcome = await ScanAsync("missing.html");

        Assert.Equal(ScanStatus.HttpError, outcome.Status);
        Assert.Equal(404, outcome.HttpStatus);
        Assert.Null(outcome.Result);
    }

    [Fact]
    public async Task ScanAsync_ClosedPort_ReturnsUnreachable()
    {
        var outcome = await Scanner().ScanAsync(FixtureServer.ClosedPortUrl(), CancellationToken.None);

        Assert.Equal(ScanStatus.Unreachable, outcome.Status);
    }

    private Task<PageScanOutcome> ScanAsync(string fileName) =>
        Scanner().ScanAsync(fixture.Server.UrlFor(fileName), CancellationToken.None);

    private PageScanner Scanner() =>
        new(fixture.Session.Browser, Options.Create(TestSettings.Scan()), NullLogger<PageScanner>.Instance);
}
