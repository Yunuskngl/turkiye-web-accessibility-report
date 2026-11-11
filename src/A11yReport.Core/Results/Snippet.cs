using System.Text.RegularExpressions;

namespace A11yReport.Core.Results;

public static partial class Snippet
{
    public const int MaxLength = 300;
    private const string Ellipsis = "…";

    public static string Truncate(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // Collapsing whitespace keeps more useful markup inside the length limit.
        var compact = Whitespace().Replace(html.Trim(), " ");
        if (compact.Length <= MaxLength)
        {
            return compact;
        }

        return compact[..(MaxLength - Ellipsis.Length)] + Ellipsis;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
