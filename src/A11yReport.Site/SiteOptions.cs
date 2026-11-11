namespace A11yReport.Site;

public sealed class SiteOptions
{
    public const string SectionName = "Report";

    public string OutputFolder { get; set; } = string.Empty;
    public string MethodologyFile { get; set; } = string.Empty;
    public string OptOutFile { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;

    public bool IsValid() =>
        new[] { OutputFolder, MethodologyFile, OptOutFile, RepositoryUrl }.All(value => !string.IsNullOrWhiteSpace(value));
}
