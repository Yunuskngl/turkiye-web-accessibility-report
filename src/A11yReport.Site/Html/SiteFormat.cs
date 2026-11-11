using System.Globalization;

namespace A11yReport.Site.Html;

/// <summary>Number and date formatting for one page language (Turkish writes "%95,0" and "48,4").</summary>
public sealed class SiteFormat(CultureInfo culture, bool percentSignFirst)
{
    public static SiteFormat English { get; } = new(CultureInfo.GetCultureInfo("en-GB"), percentSignFirst: false);
    public static SiteFormat Turkish { get; } = new(CultureInfo.GetCultureInfo("tr-TR"), percentSignFirst: true);

    public CultureInfo Culture { get; } = culture;

    public string Percent(double value)
    {
        var number = value.ToString("0.0", Culture);
        return percentSignFirst ? "%" + number : number + "%";
    }

    public string OneDecimal(double value) => value.ToString("0.0", Culture);

    public string Count(int value) => value.ToString("#,0", Culture);

    public string Date(DateTimeOffset value) => value.ToUniversalTime().ToString("d MMMM yyyy", Culture);
}
