namespace A11yReport.Site;

public sealed class SiteOptions
{
    public const string SectionName = "Report";

    public string OutputFolder { get; set; } = string.Empty;
    public string MethodologyFile { get; set; } = string.Empty;
    public string OptOutFile { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>The review file with Turkish rule explanations. Only "approved" entries are published.</summary>
    public string ExplanationsFile { get; set; } = string.Empty;

    public bool IsValid() =>
        new[] { OutputFolder, MethodologyFile, OptOutFile, RepositoryUrl, ExplanationsFile }.All(value => !string.IsNullOrWhiteSpace(value));
}
