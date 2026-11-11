using Microsoft.Playwright;

namespace A11yReport.Scanner.Scanning;

/// <summary>Owns one Playwright instance and one headless Chromium browser.</summary>
public sealed class BrowserSession : IAsyncDisposable
{
    private readonly IPlaywright _playwright;

    private BrowserSession(IPlaywright playwright, IBrowser browser)
    {
        _playwright = playwright;
        Browser = browser;
    }

    public IBrowser Browser { get; }

    public static async Task<BrowserSession> StartAsync()
    {
        var playwright = await Playwright.CreateAsync();
        try
        {
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            return new BrowserSession(playwright, browser);
        }
        catch
        {
            playwright.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.CloseAsync();
        _playwright.Dispose();
    }
}
