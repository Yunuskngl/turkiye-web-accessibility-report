namespace A11yReport.Core.Explanations;

/// <summary>Builds the prompt from axe rule metadata only.</summary>
public static class ExplanationPrompt
{
    public const string JsonSchema = """
        {
          "type": "object",
          "properties": {
            "title": { "type": "string" },
            "explanation": { "type": "string" },
            "fixExample": { "type": "string" }
          },
          "required": ["title", "explanation", "fixExample"]
        }
        """;

    public static string Build(RuleInfo rule)
    {
        var criteria = rule.Criteria.Count == 0 ? "none" : string.Join(", ", rule.Criteria);
        return $"""
            You write short accessibility guidance in Turkish for website owners and web developers in Türkiye.
            Explain the automated accessibility rule below. Use only the information given here.
            Do not invent WCAG numbers, laws, deadlines or statistics.

            Rule id: {rule.RuleId}
            Rule summary (from axe-core): {rule.Help}
            Rule description (from axe-core): {rule.Description}
            WCAG success criteria: {criteria}

            Answer in Turkish with a JSON object:
            - "title": a short plain-language name for the problem, at most 8 words.
            - "explanation": 2 or 3 sentences: what the problem is and which people it affects
              (for example screen reader users, keyboard users or people with low vision).
            - "fixExample": a minimal HTML example. First a line starting with "Hatalı:" and the wrong code,
              then a line starting with "Doğru:" and the fixed code. Keep HTML code in English.
            """;
    }
}
