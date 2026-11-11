using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Json.Serialization;
using A11yReport.Core.Results;

namespace A11yReport.Scanner.Running;

public static class ManifestWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<string> WriteAsync(RunManifest manifest, string folder)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"{manifest.Name}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(manifest, JsonOptions));
        return path;
    }
}
