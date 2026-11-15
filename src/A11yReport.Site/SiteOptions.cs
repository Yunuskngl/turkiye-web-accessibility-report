namespace A11yReport.Site;

public sealed class SiteOptions
{
    public const string SectionName = "Report";

    public string OutputFolder { get; set; } = string.Empty;
    public string MethodologyFile { get; set; } = string.Empty;
    public string MethodologyFileTurkish { get; set; } = string.Empty;
    public string PrivacyFile { get; set; } = string.Empty;
    public string PrivacyFileTurkish { get; set; } = string.Empty;
    public string OptOutFile { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>The review file with Turkish rule explanations. Only "approved" entries are published.</summary>
    public string ExplanationsFile { get; set; } = string.Empty;

    /// <summary>Domains never offered as lookup suggestions (for example a person's own name). Typing the full address still works.</summary>
    public string SuggestExcludeFile { get; set; } = string.Empty;

    /// <summary>SHA-256 hashes of domains left out of every published page and file.</summary>
    public string ExcludeHashedFile { get; set; } = string.Empty;

    public bool IsValid() =>
        new[] { OutputFolder, MethodologyFile, MethodologyFileTurkish, PrivacyFile, PrivacyFileTurkish, OptOutFile, RepositoryUrl, ExplanationsFile, SuggestExcludeFile, ExcludeHashedFile }.All(value => !string.IsNullOrWhiteSpace(value));
}
