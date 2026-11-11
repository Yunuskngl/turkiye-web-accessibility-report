using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace A11yReport.Site.Html;

/// <summary>HTML encoding. Every text that comes from data goes through Encode.</summary>
public static class HtmlText
{
    // Encodes markup characters (< > & " ') but keeps letters such as ü and ş readable.
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    public static string Encode(string? text) => Encoder.Encode(text ?? string.Empty);
}
