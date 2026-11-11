using System.Text.Json.Serialization;

namespace A11yReport.Core.Explanations;

/// <summary>An axe-core rule as axe describes it. This is the only input the language model gets.</summary>
public sealed record RuleInfo(string RuleId, string Help, string Description, IReadOnlyList<string> Criteria);

public static class ExplanationStatus
{
    public const string Draft = "draft";
    public const string Approved = "approved";
}

/// <summary>
/// A Turkish explanation of one rule. Generated as a draft; only the project owner sets
/// <see cref="Status"/> to "approved", and only approved entries are published.
/// </summary>
public sealed record RuleExplanation(
    string RuleId,
    string Help,
    string Criteria,
    string Title,
    string Explanation,
    string FixExample,
    string Status,
    string Model,
    DateTimeOffset GeneratedAt)
{
    /// <summary>Review history, for example who corrected the draft and why. Never published.</summary>
    public string Notes { get; init; } = string.Empty;

    [JsonIgnore]
    public bool IsApproved => Status == ExplanationStatus.Approved;
}

/// <summary>The three fields the model writes.</summary>
public sealed record ExplanationText(string Title, string Explanation, string FixExample);
