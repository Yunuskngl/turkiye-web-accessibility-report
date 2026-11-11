namespace A11yReport.Core.Explanations;

public sealed record GenerationSummary(int Existing, int Generated, IReadOnlyList<string> Failed);

/// <summary>
/// Creates a draft explanation once per rule. Rules that already have an entry (draft or approved)
/// are never sent again, so reviewed text is never overwritten.
/// </summary>
public sealed class ExplanationGenerator(ILlmClient llm, TimeProvider time)
{
    public async Task<GenerationSummary> GenerateMissingAsync(
        IReadOnlyList<RuleInfo> rules,
        List<RuleExplanation> explanations,
        Action<IReadOnlyList<RuleExplanation>> save,
        CancellationToken cancellationToken)
    {
        var known = explanations.Select(e => e.RuleId).ToHashSet(StringComparer.Ordinal);
        var missing = rules.Where(rule => !known.Contains(rule.RuleId)).ToList();
        var failed = new List<string>();
        foreach (var rule in missing)
        {
            var draft = await DraftAsync(rule, cancellationToken);
            if (draft is null)
            {
                failed.Add(rule.RuleId);
                continue;
            }

            explanations.Add(draft);
            save(explanations);
        }

        return new GenerationSummary(known.Count, missing.Count - failed.Count, failed);
    }

    private async Task<RuleExplanation?> DraftAsync(RuleInfo rule, CancellationToken cancellationToken)
    {
        var answer = await llm.GenerateJsonAsync(ExplanationPrompt.Build(rule), ExplanationPrompt.JsonSchema, cancellationToken);
        var text = ExplanationParser.Parse(answer);
        return text is null
            ? null
            : new RuleExplanation(rule.RuleId, rule.Help, string.Join(", ", rule.Criteria), text.Title, text.Explanation,
                text.FixExample, ExplanationStatus.Draft, llm.ModelName, time.GetUtcNow());
    }
}
