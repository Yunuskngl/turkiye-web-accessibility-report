using A11yReport.Core.Explanations;
using A11yReport.Core.Results;
using A11yReport.Scanner.Scanning;
using A11yReport.Site;
using A11yReport.Site.Html;
using A11yReport.Site.Lookup;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace A11yReport.Tests.Integration;

/// <summary>
/// Stage 4 and 5 rules: our own scanner finds zero failures on every page of the report site, and
/// the lookup page finds a site only by its typed address (and never an opted-out one).
/// </summary>
[Trait("Category", "Integration")]
public sealed class ReportSiteAccessibilityTests : IClassFixture<ScannerFixture>, IDisposable
{
    private const string Methodology = """
        # Methodology

        Some text with a [link](https://example.com) and `code`.

        | Site | Errors |
        |---|---|
        | example | 3 |

        ## Opting out

        - first item
        - second item
        """;

    private readonly ScannerFixture _fixture;
    private readonly string _folder = Directory.CreateTempSubdirectory("a11yreport-site-").FullName;
    private readonly FixtureServer _server;

    public ReportSiteAccessibilityTests(ScannerFixture fixture)
    {
        _fixture = fixture;
        GenerateSite(optOut: ["c.com.tr"]);
        _server = new FixtureServer(homePage: "index.html", rootFolder: _folder);
    }

    public void Dispose()
    {
        _server.Dispose();
        Directory.Delete(_folder, recursive: true);
    }

    [Theory]
    [InlineData("index.html")]
    [InlineData("lookup.html")]
    [InlineData("methodology.html")]
    [InlineData("tr/index.html")]
    [InlineData("tr/lookup.html")]
    [InlineData("tr/methodology.html")]
    public async Task GeneratedPage_HasNoDetectedFailures(string page)
    {
        var scanner = new PageScanner(_fixture.Session.Browser, Options.Create(TestSettings.Scan()), NullLogger<PageScanner>.Instance);

        var outcome = await scanner.ScanAsync(_server.UrlFor(page), CancellationToken.None);

        Assert.Equal(ScanStatus.Done, outcome.Status);
        Assert.All(outcome.Result!.Viewports, viewport => Assert.Empty(viewport.Violations));
    }

    [Fact]
    public async Task Lookup_TypedAddress_ShowsResultFocusesHeadingAndHasNoFailures()
    {
        await using var context = await _fixture.Session.Browser.NewContextAsync();
        var page = await LookUpAsync(context, "https://WWW.A.gov.tr/some/path?x=1");

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Result for a.gov.tr" })).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("Low contrast text (color-contrast)");
        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("1.4.3");
        await Assertions.Expect(page.Locator("#lookup-result section[lang='tr']")).ToContainTextAsync("Düşük kontrastlı metin");
        await Assertions.Expect(page.Locator("#lookup-result")).Not.ToContainTextAsync("Taslak başlık");
        var axe = await page.RunAxe(new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "tag", Values = TestSettings.Scan().AxeTags } });
        Assert.Empty(axe.Violations);
    }

    [Fact]
    public async Task TurkishLookup_ShowsTurkishResultAndHasNoFailures()
    {
        await using var context = await _fixture.Session.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(_server.UrlFor("tr/lookup.html").ToString());
        await page.GetByLabel("Web sitesi adresi").FillAsync("a.gov.tr");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sonucu göster" }).ClickAsync();

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "a.gov.tr için sonuç" })).ToBeFocusedAsync();
        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("Sektör: Kamu");
        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("Düşük kontrastlı metin (color-contrast)");
        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("Kasım 2026");
        var axe = await page.RunAxe(new AxeRunOptions { RunOnly = new RunOnlyOptions { Type = "tag", Values = TestSettings.Scan().AxeTags } });
        Assert.Empty(axe.Violations);
    }

    [Fact]
    public async Task Lookup_Subdomain_FindsTheSite()
    {
        await using var context = await _fixture.Session.Browser.NewContextAsync();
        var page = await LookUpAsync(context, "obs.uni.edu.tr");

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Result for uni.edu.tr" })).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("unknown.com.tr")]
    [InlineData("c.com.tr")]
    public async Task Lookup_UnknownOrOptedOutSite_ShowsNoResult(string address)
    {
        await using var context = await _fixture.Session.Browser.NewContextAsync();
        var page = await LookUpAsync(context, address);

        await Assertions.Expect(page.Locator("#lookup-status")).ToContainTextAsync("No result for " + address);
        await Assertions.Expect(page.Locator("#lookup-result h2")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Lookup_SiteThatWasNotChecked_SaysSoAndShowsNoNumbers()
    {
        await using var context = await _fixture.Session.Browser.NewContextAsync();
        var page = await LookUpAsync(context, "d.com.tr");

        await Assertions.Expect(page.Locator("#lookup-result")).ToContainTextAsync("was not checked: Did not load in time");
        await Assertions.Expect(page.Locator("#lookup-result table")).ToHaveCountAsync(0);
    }

    [Fact]
    public void OptedOutSite_HasNoFileAndIsNotCounted()
    {
        Assert.False(File.Exists(Path.Combine(_folder, LookupFiles.FolderName, LookupFiles.FileNameFor("c.com.tr"))));
        Assert.True(File.Exists(Path.Combine(_folder, LookupFiles.FolderName, LookupFiles.FileNameFor("b.com.tr"))));
        Assert.DoesNotContain("c.com.tr", File.ReadAllText(Path.Combine(_folder, "report.json")), StringComparison.Ordinal);
    }

    private async Task<IPage> LookUpAsync(IBrowserContext context, string address)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync(_server.UrlFor("lookup.html").ToString());
        await page.GetByLabel("Website address").FillAsync(address);
        await page.GetByRole(AriaRole.Button, new() { Name = "Show result" }).ClickAsync();
        return page;
    }

    private void GenerateSite(HashSet<string> optOut)
    {
        var methodologyFile = Path.Combine(_folder, "methodology.md");
        File.WriteAllText(methodologyFile, Methodology);
        var explanationsFile = Path.Combine(_folder, "rules.tr.json");
        ExplanationFile.Save(explanationsFile,
        [
            Unit.ExplanationTests.Explanation("color-contrast", ExplanationStatus.Approved, "Düşük kontrastlı metin"),
            Unit.ExplanationTests.Explanation("image-alt", ExplanationStatus.Draft, "Taslak başlık"),
        ]);
        var options = new SiteOptions
        {
            OutputFolder = _folder,
            MethodologyFile = methodologyFile,
            MethodologyFileTurkish = methodologyFile,
            OptOutFile = "unused",
            RepositoryUrl = "https://github.com/example/repo",
            ExplanationsFile = explanationsFile,
        };
        var outcomes = ReportSamples.Outcomes();
        var data = A11yReport.Core.Report.ReportBuilder.Build(ReportSamples.Run, outcomes, optOut);
        var names = RuleNames.From(ExplanationFile.Load(explanationsFile));
        new SiteGenerator(Options.Create(options)).Generate(data, LookupBuilder.Build(ReportSamples.Run, outcomes, optOut, names), names,
            isPilot: true);
    }
}
