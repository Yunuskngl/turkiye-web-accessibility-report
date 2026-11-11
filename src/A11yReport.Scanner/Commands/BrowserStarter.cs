using A11yReport.Scanner.Scanning;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Commands;

internal static class BrowserStarter
{
    public const string InstallHint =
        "Chromium could not start. Install it with: dotnet run --project src/A11yReport.Scanner -- install-browser";

    /// <summary>Returns null (after telling the person what to do) when Chromium is missing.</summary>
    public static async Task<BrowserSession?> TryStartAsync(ILogger logger)
    {
        try
        {
            return await BrowserSession.StartAsync();
        }
        catch (PlaywrightException exception)
        {
            Log.BrowserStartFailed(logger, exception);
            await Console.Error.WriteLineAsync(InstallHint);
            return null;
        }
    }
}
