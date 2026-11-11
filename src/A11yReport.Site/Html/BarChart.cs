using System.Globalization;
using System.Text;

namespace A11yReport.Site.Html;

public sealed record Bar(string Label, double Value, string ValueText);

/// <summary>
/// Horizontal bar chart as inline SVG. It has a title and description for screen readers, and
/// every chart on the page is followed by a table with the same numbers.
/// </summary>
public static class BarChart
{
    private const int Width = 760;
    private const int LabelWidth = 330;
    private const int BarAreaWidth = 340;
    private const int RowHeight = 34;
    private const int BarHeight = 22;
    private const int TopPadding = 8;
    private const int AxisHeight = 34;

    public static string Render(string id, string title, string description, IReadOnlyList<Bar> bars, string axisLabel)
    {
        var height = TopPadding + (bars.Count * RowHeight) + AxisHeight;
        var max = bars.Count == 0 ? 1 : Math.Max(bars.Max(bar => bar.Value), double.Epsilon);
        var svg = new StringBuilder();
        svg.Append(CultureInfo.InvariantCulture,
            $"""<svg class="chart" role="img" viewBox="0 0 {Width} {height}" aria-labelledby="{id}-title {id}-desc">""");
        svg.Append(CultureInfo.InvariantCulture, $"""<title id="{id}-title">{HtmlText.Encode(title)}</title>""");
        svg.Append(CultureInfo.InvariantCulture, $"""<desc id="{id}-desc">{HtmlText.Encode(description)}</desc>""");
        for (var i = 0; i < bars.Count; i++)
        {
            svg.Append(Row(bars[i], TopPadding + (i * RowHeight), max));
        }

        svg.Append(Axis(height, axisLabel));
        svg.Append("</svg>");
        return svg.ToString();
    }

    private static string Row(Bar bar, int y, double max)
    {
        var length = Math.Max(1, (int)Math.Round(BarAreaWidth * bar.Value / max));
        var textY = y + (BarHeight / 2) + 5;
        return string.Create(CultureInfo.InvariantCulture,
            $"""<text x="{LabelWidth - 10}" y="{textY}" text-anchor="end" class="chart-label">{HtmlText.Encode(bar.Label)}</text>""" +
            $"""<rect x="{LabelWidth}" y="{y}" width="{length}" height="{BarHeight}" class="chart-bar"></rect>""" +
            $"""<text x="{LabelWidth + length + 8}" y="{textY}" class="chart-value">{HtmlText.Encode(bar.ValueText)}</text>""");
    }

    private static string Axis(int height, string axisLabel) =>
        string.Create(CultureInfo.InvariantCulture,
            $"""<text x="{LabelWidth}" y="{height - 10}" class="chart-axis">{HtmlText.Encode(axisLabel)}</text>""");
}
