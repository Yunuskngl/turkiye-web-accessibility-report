using A11yReport.Scanner;
using A11yReport.Storage;

namespace A11yReport.Tests;

internal static class TestSettings
{
    public const string RobotsToken = "TurkiyeWebA11yReport";

    public static ScanOptions Scan() => new()
    {
        NavigationTimeoutSeconds = 10,
        LoadWaitSeconds = 5,
        CheckTimeoutSeconds = 30,
        SettleMilliseconds = 0,
        ResizeSettleMilliseconds = 0,
        Locale = "tr-TR",
        UserAgentProduct = RobotsToken + "/test",
        MethodologyUrl = "http://127.0.0.1/methodology",
        Desktop = new() { Name = "Desktop", Width = 1280, Height = 800 },
        Mobile = new() { Name = "Mobile", Width = 390, Height = 844 },
        AxeTags = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22aa"],
    };

    public static RunOptions Run(string manifestFolder) => new()
    {
        Concurrency = 2,
        DelayBetweenSitesSeconds = 1,
        RobotsTimeoutSeconds = 5,
        ManifestFolder = manifestFolder,
    };

    public static StorageOptions Storage(string databasePath) => new()
    {
        DatabasePath = databasePath,
        MaxNodesPerRule = 2,
    };
}
