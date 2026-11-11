using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using A11yReport.Core.Report;
using A11yReport.Site.Pages;
using Microsoft.Extensions.Options;

namespace A11yReport.Site;

/// <summary>Writes the static site: index.html, methodology.html, report.json and styles.css.</summary>
public sealed class SiteGenerator(IOptions<SiteOptions> options)
{
    private const string AssetsFolder = "assets";
    private const string StylesFile = "styles.css";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly SiteOptions _options = options.Value;

    public string Generate(ReportData data, bool isPilot)
    {
        var folder = Path.GetFullPath(_options.OutputFolder);
        Directory.CreateDirectory(folder);
        Write(folder, "index.html", ReportPage.Render(data, isPilot, _options.RepositoryUrl));
        Write(folder, "methodology.html", MethodologyPage.Render(File.ReadAllText(_options.MethodologyFile), _options.RepositoryUrl));
        Write(folder, "report.json", JsonSerializer.Serialize(data, JsonOptions));
        File.Copy(Path.Combine(AppContext.BaseDirectory, AssetsFolder, StylesFile), Path.Combine(folder, StylesFile), overwrite: true);
        return folder;
    }

    private static void Write(string folder, string fileName, string content) =>
        File.WriteAllText(Path.Combine(folder, fileName), content);
}
