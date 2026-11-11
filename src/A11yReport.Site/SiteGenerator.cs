using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using A11yReport.Core.Explanations;
using A11yReport.Core.Report;
using A11yReport.Site.Html;
using A11yReport.Site.Lookup;
using A11yReport.Site.Pages;
using Microsoft.Extensions.Options;

namespace A11yReport.Site;

/// <summary>
/// Writes the static site: English pages at the root and Turkish pages in /tr/, plus report.json,
/// per-site lookup files, approved explanations and assets (shared by both languages).
/// </summary>
public sealed class SiteGenerator(IOptions<SiteOptions> options)
{
    private const string AssetsFolder = "assets";
    private static readonly string[] AssetFiles = ["styles.css", "lookup.js"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly SiteOptions _options = options.Value;

    public string Generate(ReportData data, IReadOnlyList<LookupSite> lookupSites, RuleNames ruleNames, bool isPilot)
    {
        var folder = Path.GetFullPath(_options.OutputFolder);
        Directory.CreateDirectory(folder);
        WritePages(folder, SiteLanguages.English, _options.MethodologyFile, data, ruleNames, isPilot);
        WritePages(Path.Combine(folder, SiteLanguages.Turkish.Folder), SiteLanguages.Turkish, _options.MethodologyFileTurkish,
            data, ruleNames, isPilot);
        Write(folder, "report.json", JsonSerializer.Serialize(data, JsonOptions));
        LookupFiles.WriteAll(folder, lookupSites);
        Write(folder, "explanations.tr.json", JsonSerializer.Serialize(ApprovedExplanations(), JsonOptions));
        CopyAssets(folder);
        return folder;
    }

    private void WritePages(string folder, SiteTexts texts, string methodologyFile, ReportData data, RuleNames ruleNames, bool isPilot)
    {
        Directory.CreateDirectory(folder);
        Write(folder, "index.html", new ReportPage(texts, ruleNames).Render(data, isPilot, _options.RepositoryUrl));
        Write(folder, "lookup.html", LookupPage.Render(texts, data.Run, isPilot, _options.RepositoryUrl));
        Write(folder, "methodology.html", MethodologyPage.Render(texts, File.ReadAllText(methodologyFile), _options.RepositoryUrl));
    }

    /// <summary>Drafts written by the language model stay private until the owner approves them.</summary>
    private List<PublishedExplanation> ApprovedExplanations() =>
        ExplanationFile.Load(_options.ExplanationsFile)
            .Where(e => e.IsApproved)
            .Select(e => new PublishedExplanation(e.RuleId, e.Title, e.Explanation, e.FixExample))
            .ToList();

    private sealed record PublishedExplanation(string RuleId, string Title, string Explanation, string FixExample);

    private static void CopyAssets(string folder)
    {
        foreach (var file in AssetFiles)
        {
            File.Copy(Path.Combine(AppContext.BaseDirectory, AssetsFolder, file), Path.Combine(folder, file), overwrite: true);
        }
    }

    private static void Write(string folder, string fileName, string content) =>
        File.WriteAllText(Path.Combine(folder, fileName), content);
}
