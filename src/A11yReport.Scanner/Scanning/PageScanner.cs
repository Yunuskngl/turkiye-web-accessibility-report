using A11yReport.Core.Results;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Scanning;

/// <summary>Loads one page once and runs axe on it in the desktop and mobile viewports.</summary>
public sealed class PageScanner(IBrowser browser, IOptions<ScanOptions> options, ILogger<PageScanner> logger)
{
    private const string AxeTagFilter = "tag";
    private const string ScannedMessage = "Scanned.";
    private readonly ScanOptions _options = options.Value;

    public async Task<PageScanOutcome> ScanAsync(Uri url, CancellationToken cancellationToken)
    {
        await using var context = await browser.NewContextAsync(CreateContextOptions());
        var page = await context.NewPageAsync();
        var counter = RequestCounter.Attach(page);
        try
        {
            var result = await ScanPageAsync(page, url, cancellationToken);
            return new PageScanOutcome(ScanStatus.Done, ScannedMessage, counter.Counts, result.HttpStatus, result);
        }
        catch (ScanException exception)
        {
            Log.PageScanFailed(logger, exception, url);
            return new PageScanOutcome(exception.Status, exception.Message, counter.Counts, exception.HttpStatus);
        }
    }

    private async Task<PageScanResult> ScanPageAsync(IPage page, Uri url, CancellationToken cancellationToken)
    {
        var response = await OpenAsync(page, url, cancellationToken);
        var facts = await PageFactsReader.ReadAsync(page);
        var desktop = await RunAxeAsync(page, _options.Desktop, cancellationToken);

        // Resizing the same page keeps it at one page load per site.
        await page.SetViewportSizeAsync(_options.Mobile.Width, _options.Mobile.Height);
        await Task.Delay(_options.ResizeSettleMilliseconds, cancellationToken);
        var mobile = await RunAxeAsync(page, _options.Mobile, cancellationToken);

        return new PageScanResult(
            url, new Uri(page.Url), response.Status, desktop.Version,
            DateTimeOffset.UtcNow, [desktop.Result, mobile.Result], facts);
    }

    private BrowserNewContextOptions CreateContextOptions() => new()
    {
        ViewportSize = new ViewportSize { Width = _options.Desktop.Width, Height = _options.Desktop.Height },
        Locale = _options.Locale,
        UserAgent = _options.UserAgent,
    };

    private async Task<IResponse> OpenAsync(IPage page, Uri url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = await NavigateAsync(page, url);
        EnsureHtmlPage(response);

        Log.PageLoaded(logger, url, response.Status);
        await Task.Delay(_options.SettleMilliseconds, cancellationToken);
        return response;
    }

    private async Task<IResponse> NavigateAsync(IPage page, Uri url)
    {
        try
        {
            var response = await page.GotoAsync(url.ToString(), new()
            {
                Timeout = (float)TimeSpan.FromSeconds(_options.NavigationTimeoutSeconds).TotalMilliseconds,
                WaitUntil = WaitUntilState.Load,
            });
            return response ?? throw new ScanException(ScanStatus.Failed, "The page returned no response.");
        }
        catch (TimeoutException exception)
        {
            throw new ScanException(ScanStatus.Timeout, FailureClassifier.Describe(ScanStatus.Timeout), null, exception);
        }
        catch (PlaywrightException exception)
        {
            var status = FailureClassifier.FromChromiumMessage(exception.Message);
            throw new ScanException(status, FailureClassifier.Describe(status), null, exception);
        }
    }

    private static void EnsureHtmlPage(IResponse response)
    {
        if (!response.Ok)
        {
            var status = FailureClassifier.FromHttpStatus(response.Status);
            throw new ScanException(status, FailureClassifier.Describe(status, response.Status), response.Status);
        }

        var contentType = response.Headers.GetValueOrDefault("content-type", string.Empty);
        if (!contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
        {
            throw new ScanException(ScanStatus.NonHtml, FailureClassifier.Describe(ScanStatus.NonHtml), response.Status);
        }
    }

    private async Task<(ViewportResult Result, string Version)> RunAxeAsync(
        IPage page, ViewportOptions viewport, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var runOptions = new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = AxeTagFilter, Values = _options.AxeTags },
        };

        try
        {
            var result = await page.RunAxe(runOptions);
            Log.AxeFinished(logger, result.Violations?.Length ?? 0, viewport.Name);
            return (AxeResultMapper.ToViewportResult(result, viewport), result.TestEngine?.Version ?? "unknown");
        }
        catch (PlaywrightException exception)
        {
            throw new ScanException(ScanStatus.Failed, "axe could not run on this page.", null, exception);
        }
    }
}
