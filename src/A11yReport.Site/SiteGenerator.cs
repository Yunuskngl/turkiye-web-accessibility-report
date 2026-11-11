using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using A11yReport.Core.Explanations;
using A11yReport.Core.Report;
using A11yReport.Site.Lookup;
using A11yReport.Site.Pages;
using Microsoft.Extensions.Options;

namespace A11yReport.Site;

/// <summary>Writes the static site: report, lookup and methodology pages, report.json, per-site lookup files and assets.</summary>
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

    public string Generate(ReportData data, IReadOnlyList<LookupSite> lookupSites, bool isPilot)
    {
        var folder = Path.GetFullPath(_options.OutputFolder);
        Directory.CreateDirectory(folder);
        Write(folder, "index.html", ReportPage.Render(data, isPilot, _options.RepositoryUrl));
        Write(folder, "lookup.html", LookupPage.Render(data.Run, isPilot, _options.RepositoryUrl));
        Write(folder, "methodology.html", MethodologyPage.Render(File.ReadAllText(_options.MethodologyFile), _options.RepositoryUrl));
        Write(folder, "report.json", JsonSerializer.Serialize(data, JsonOptions));
        LookupFiles.WriteAll(folder, lookupSites);
        Write(folder, "explanations.tr.json", JsonSerializer.Serialize(ApprovedExplanations(), JsonOptions));
        CopyAssets(folder);
        return folder;
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
