using A11yReport.Core.Explanations;

namespace A11yReport.Site.Html;

/// <summary>Display names of axe rules per language. Turkish names are the titles of approved explanations.</summary>
public sealed class RuleNames(IReadOnlyDictionary<string, string> turkishTitles)
{
    public static RuleNames From(IEnumerable<RuleExplanation> explanations) =>
        new(explanations.Where(e => e.IsApproved).ToDictionary(e => e.RuleId, e => e.Title, StringComparer.Ordinal));

    public static string English(string ruleId, string help) => Labels.Rule(ruleId, help);

    public string Turkish(string ruleId, string help) =>
        turkishTitles.TryGetValue(ruleId, out var title) ? title : Labels.Rule(ruleId, help);

    public string For(SiteTexts texts, string ruleId, string help) =>
        texts.LangCode == SiteLanguages.Turkish.LangCode ? Turkish(ruleId, help) : English(ruleId, help);
}
