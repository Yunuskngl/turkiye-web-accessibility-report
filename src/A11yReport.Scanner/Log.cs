using A11yReport.Core.Results;
using Microsoft.Extensions.Logging;

namespace A11yReport.Scanner;

/// <summary>
/// Log messages defined once, with compile-time generated code (faster than string templates at runtime).
/// Messages never contain page content. Full error details are logged at Debug level only.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Loaded {Url} with HTTP {Status}")]
    public static partial void PageLoaded(ILogger logger, Uri url, int status);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Url}: load event not reached after {Seconds} more seconds; scanning anyway")]
    public static partial void LoadEventNotReached(ILogger logger, Uri url, int seconds);

    [LoggerMessage(Level = LogLevel.Debug, Message = "axe found {Count} failed rules in the {Viewport} viewport")]
    public static partial void AxeFinished(ILogger logger, int count, string viewport);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Page scan of {Url} failed")]
    public static partial void PageScanFailed(ILogger logger, Exception exception, Uri url);

    [LoggerMessage(Level = LogLevel.Debug, Message = "robots.txt request {Url} failed")]
    public static partial void RobotsFailed(ILogger logger, Exception exception, Uri url);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Language model request failed")]
    public static partial void LlmFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Chromium could not start")]
    public static partial void BrowserStartFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Run {Run}: {Total} sites in the list, {Pending} to scan now (resumed: {IsResume})")]
    public static partial void RunStarted(ILogger logger, string run, int total, int pending, bool isResume);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrying {Count} sites with transient errors once")]
    public static partial void RetryStarted(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Done}/{Total}] {Domain}: {Status}")]
    public static partial void SiteFinished(ILogger logger, int done, int total, string domain, ScanStatus status);

    [LoggerMessage(Level = LogLevel.Information, Message = "Run manifest written to {Path}")]
    public static partial void ManifestWritten(ILogger logger, string path);
}
