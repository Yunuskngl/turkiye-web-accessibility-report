using A11yReport.Core.Results;
using A11yReport.Scanner.Scanning;
using A11yReport.Site;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace A11yReport.Tests.Integration;

/// <summary>Stage 4 rule: our own scanner must find zero failures on the report site, in both viewports.</summary>
[Trait("Category", "Integration")]
public sealed class ReportSiteAccessibilityTests(ScannerFixture fixture) : IClassFixture<ScannerFixture>, IDisposable
{
    private const string Methodology = """
        # Methodology

        Some text with a [link](https://example.com) and `code`.

        | Site | Errors |
        |---|---|
        | example | 3 |

        - first item
        - second item
        """;

    private readonly string _folder = Directory.CreateTempSubdirectory("a11yreport-site-").FullName;

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Theory]
    [InlineData("index.html")]
    [InlineData("methodology.html")]
    public async Task GeneratedPage_HasNoDetectedFailures(string page)
    {
        GenerateSite();
        using var server = new FixtureServer(homePage: "index.html", rootFolder: _folder);
        var scanner = new PageScanner(fixture.Session.Browser, Options.Create(TestSettings.Scan()), NullLogger<PageScanner>.Instance);

        var outcome = await scanner.ScanAsync(server.UrlFor(page), CancellationToken.None);

        Assert.Equal(ScanStatus.Done, outcome.Status);
        Assert.All(outcome.Result!.Viewports, viewport => Assert.Empty(viewport.Violations));
    }

    private void GenerateSite()
    {
        var methodologyFile = Path.Combine(_folder, "methodology.md");
        File.WriteAllText(methodologyFile, Methodology);
        var options = new SiteOptions
        {
            OutputFolder = _folder,
            MethodologyFile = methodologyFile,
            OptOutFile = "unused",
            RepositoryUrl = "https://github.com/example/repo",
        };
        new SiteGenerator(Options.Create(options)).Generate(ReportSamples.Data(), isPilot: true);
    }
}
