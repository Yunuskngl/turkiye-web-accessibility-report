namespace A11yReport.Scanner;

public sealed class ScanOptions
{
    public const string SectionName = "Scan";

    /// <summary>Time limit until the HTML document is ready (DOMContentLoaded).</summary>
    public int NavigationTimeoutSeconds { get; set; }

    /// <summary>Extra time we wait for the full load event; after it we scan anyway.</summary>
    public int LoadWaitSeconds { get; set; }

    public int SettleMilliseconds { get; set; }

    /// <summary>Time limit for each script step inside the page (reading page facts, one axe run).</summary>
    public int CheckTimeoutSeconds { get; set; }
    public int ResizeSettleMilliseconds { get; set; }
    public string Locale { get; set; } = string.Empty;

    /// <summary>Product token and version, for example "TurkiyeWebA11yReport/0.2". Also used to match robots.txt groups.</summary>
    public string UserAgentProduct { get; set; } = string.Empty;

    /// <summary>Public methodology page linked from the User-Agent. Required before scanning real sites.</summary>
    public string MethodologyUrl { get; set; } = string.Empty;

    public ViewportOptions Desktop { get; set; } = new();
    public ViewportOptions Mobile { get; set; } = new();
    public List<string> AxeTags { get; set; } = [];

    public string RobotsToken => UserAgentProduct.Split('/')[0];

    public string UserAgent => string.IsNullOrWhiteSpace(MethodologyUrl)
        ? $"Mozilla/5.0 (compatible; {UserAgentProduct})"
        : $"Mozilla/5.0 (compatible; {UserAgentProduct}; +{MethodologyUrl})";

    public bool IsValid() =>
        NavigationTimeoutSeconds > 0
        && LoadWaitSeconds >= 0
        && CheckTimeoutSeconds > 0
        && SettleMilliseconds >= 0
        && ResizeSettleMilliseconds >= 0
        && !string.IsNullOrWhiteSpace(Locale)
        && !string.IsNullOrWhiteSpace(UserAgentProduct)
        && Desktop.IsValid()
        && Mobile.IsValid()
        && AxeTags.Count > 0;
}

public sealed class ViewportOptions
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }

    public bool IsValid() => !string.IsNullOrWhiteSpace(Name) && Width > 0 && Height > 0;
}

public sealed class RunOptions
{
    public const string SectionName = "Run";
    public const int MaxConcurrency = 4;

    public int Concurrency { get; set; }
    public int DelayBetweenSitesSeconds { get; set; }
    public int RobotsTimeoutSeconds { get; set; }
    public string ManifestFolder { get; set; } = string.Empty;

    public bool IsValid() =>
        Concurrency is >= 1 and <= MaxConcurrency
        && DelayBetweenSitesSeconds >= 1
        && RobotsTimeoutSeconds > 0
        && !string.IsNullOrWhiteSpace(ManifestFolder);
}

public sealed class DataOptions
{
    public const string SectionName = "Data";

    public string SitesFile { get; set; } = string.Empty;
    public string SitesSourceFile { get; set; } = string.Empty;
    public string SectorsFile { get; set; } = string.Empty;
    public string IncludeFile { get; set; } = string.Empty;

    /// <summary>Domains left out of the report on purpose (gambling, adult, piracy), with a reason per row.</summary>
    public string ExcludeFile { get; set; } = string.Empty;
    public string OptOutFile { get; set; } = string.Empty;
    public string OtherTldCandidatesFile { get; set; } = string.Empty;
    public int MaxSites { get; set; }
    public int OtherTldCandidateMaxBucket { get; set; }

    public bool IsValid() =>
        new[] { SitesFile, SitesSourceFile, SectorsFile, IncludeFile, ExcludeFile, OptOutFile, OtherTldCandidatesFile }
            .All(path => !string.IsNullOrWhiteSpace(path))
        && MaxSites > 0
        && OtherTldCandidateMaxBucket > 0;
}
