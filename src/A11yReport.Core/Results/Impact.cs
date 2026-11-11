namespace A11yReport.Core.Results;

public static class Impact
{
    public const string Critical = "critical";
    public const string Serious = "serious";
    public const string Moderate = "moderate";
    public const string Minor = "minor";
    public const string Unknown = "unknown";

    private static readonly string[] Order = [Critical, Serious, Moderate, Minor];

    /// <summary>Lower rank means more severe. Unknown impacts sort last.</summary>
    public static int Rank(string? impact)
    {
        var index = Array.IndexOf(Order, impact);
        return index >= 0 ? index : Order.Length;
    }
}
