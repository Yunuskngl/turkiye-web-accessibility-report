using A11yReport.Core.Explanations;
using A11yReport.Core.Results;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Explanations;

/// <summary>Reads rule descriptions from axe-core itself, on an empty local page (no website is loaded).</summary>
public static class AxeRuleCatalog
{
    private const string EmptyPage = "<!DOCTYPE html><html lang=\"en\"><head><title>axe rules</title></head><body></body></html>";

    public static async Task<IReadOnlyList<RuleInfo>> ReadAsync(IBrowser browser, IReadOnlySet<string> ruleIds, IList<string> tags)
    {
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.SetContentAsync(EmptyPage);
        var rules = await page.GetAxeRules(tags);
        return rules
            .Where(rule => ruleIds.Contains(rule.RuleId))
            .Select(rule => new RuleInfo(rule.RuleId, rule.Help, rule.Description, WcagCriteria.FromTags(rule.Tags)))
            .OrderBy(rule => rule.RuleId, StringComparer.Ordinal)
            .ToList();
    }
}
