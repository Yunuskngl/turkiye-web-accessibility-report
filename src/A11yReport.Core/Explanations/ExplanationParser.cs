using System.Text.Json;

namespace A11yReport.Core.Explanations;

/// <summary>Validates the model's answer. Anything unexpected is rejected rather than stored.</summary>
public static class ExplanationParser
{
    public const int MaxTitleLength = 100;
    public const int MaxTextLength = 800;

    /// <summary>Returns null when the answer is not valid JSON, misses a field, or is too long.</summary>
    public static ExplanationText? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var text = new ExplanationText(Read(root, "title"), Read(root, "explanation"), Read(root, "fixExample"));
            return IsValid(text) ? text : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Read(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()!.Trim()
            : string.Empty;

    private static bool IsValid(ExplanationText text) =>
        text.Title.Length is > 0 and <= MaxTitleLength
        && text.Explanation.Length is > 0 and <= MaxTextLength
        && text.FixExample.Length is > 0 and <= MaxTextLength
        && HasDifferentWrongAndRightCode(text.FixExample);

    /// <summary>
    /// The pilot drafts showed small models returning cut-off examples or the same code twice.
    /// A usable example has a "Hatalı:" part and a different "Doğru:" part.
    /// </summary>
    public static bool HasDifferentWrongAndRightCode(string fixExample)
    {
        var wrongAt = fixExample.IndexOf(WrongMarker, StringComparison.Ordinal);
        var rightAt = fixExample.IndexOf(RightMarker, StringComparison.Ordinal);
        if (wrongAt < 0 || rightAt <= wrongAt)
        {
            return false;
        }

        var wrong = fixExample[(wrongAt + WrongMarker.Length)..rightAt].Trim();
        var right = fixExample[(rightAt + RightMarker.Length)..].Trim();
        return wrong.Length > 0 && right.Length > 0 && wrong != right;
    }

    private const string WrongMarker = "Hatalı:";
    private const string RightMarker = "Doğru:";
}
