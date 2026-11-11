using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace A11yReport.Core.Explanations;

/// <summary>The review file (data/explanations/rules.tr.json): one entry per rule, edited by hand.</summary>
public static class ExplanationFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static List<RuleExplanation> Load(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<List<RuleExplanation>>(File.ReadAllText(path), JsonOptions) ?? []
            : [];

    public static void Save(string path, IEnumerable<RuleExplanation> explanations)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var sorted = explanations.OrderBy(e => e.RuleId, StringComparer.Ordinal).ToList();
        File.WriteAllText(path, JsonSerializer.Serialize(sorted, JsonOptions) + Environment.NewLine);
    }
}
