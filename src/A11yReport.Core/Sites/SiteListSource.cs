using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace A11yReport.Core.Sites;

/// <summary>Where data/sites.csv came from, so every run can name its exact list version.</summary>
public sealed record SiteListSource(
    string Source,
    string SnapshotFile,
    string SnapshotSha256,
    DateTimeOffset CreatedAt,
    int SiteCount,
    int MaxSites)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public string Describe() => $"{Source}; snapshot {SnapshotFile} (sha256 {SnapshotSha256})";

    public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));

    public static SiteListSource Load(string path) =>
        JsonSerializer.Deserialize<SiteListSource>(File.ReadAllText(path), JsonOptions)
        ?? throw new InvalidDataException($"{path} is empty.");
}
