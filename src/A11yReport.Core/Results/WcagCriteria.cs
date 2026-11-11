using System.Text.RegularExpressions;

namespace A11yReport.Core.Results;

public static partial class WcagCriteria
{
    /// <summary>
    /// Turns axe tags such as "wcag143" or "wcag2411" into success criteria such as "1.4.3" or "2.4.11".
    /// Level tags such as "wcag2aa" are ignored.
    /// </summary>
    public static IReadOnlyList<string> FromTags(IEnumerable<string> tags)
    {
        return tags
            .Select(tag => CriterionTag().Match(tag))
            .Where(match => match.Success)
            .Select(match => $"{match.Groups[1].Value}.{match.Groups[2].Value}.{match.Groups[3].Value}")
            .Distinct()
            .ToList();
    }

    // Principle and guideline are single digits in WCAG 2.x; the criterion number can have two.
    [GeneratedRegex(@"^wcag(\d)(\d)(\d{1,2})$")]
    private static partial Regex CriterionTag();
}
