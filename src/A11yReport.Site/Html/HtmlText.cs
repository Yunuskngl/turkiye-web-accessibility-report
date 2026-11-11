using System.Globalization;
using System.Net;

namespace A11yReport.Site.Html;

/// <summary>Encoding and number formatting for the generated pages. Every text from data goes through Encode.</summary>
public static class HtmlText
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Encode(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

    public static string Percent(double value) => value.ToString("0.0", Culture) + "%";

    public static string OneDecimal(double value) => value.ToString("0.0", Culture);

    public static string Count(int value) => value.ToString("#,0", Culture);

    public static string Date(DateTimeOffset value) => value.ToUniversalTime().ToString("d MMMM yyyy", Culture);
}
