namespace A11yReport.Core.Robots;

/// <summary>
/// robots.txt rules as defined in RFC 9309: groups by user-agent, allow/disallow rules with
/// "*" and "$" wildcards, longest match wins, and allow wins a tie.
/// </summary>
public sealed class RobotsRules
{
    private const string AnyAgent = "*";
    private readonly List<RobotsGroup> _groups;

    private RobotsRules(List<RobotsGroup> groups) => _groups = groups;

    public static RobotsRules AllowAll { get; } = new([]);

    public static RobotsRules Parse(string text) => new(RobotsParser.ParseGroups(text));

    public bool IsAllowed(string productToken, string path)
    {
        var rules = RulesFor(productToken);
        var best = rules
            .Where(rule => rule.Matches(path))
            .OrderByDescending(rule => rule.Pattern.Length)
            .ThenByDescending(rule => rule.Allow)
            .FirstOrDefault();
        return best is null || best.Allow;
    }

    private List<RobotsRule> RulesFor(string productToken)
    {
        var own = _groups.Where(group => group.Agents.Contains(productToken, StringComparer.OrdinalIgnoreCase)).ToList();
        var groups = own.Count > 0 ? own : _groups.Where(group => group.Agents.Contains(AnyAgent)).ToList();
        return groups.SelectMany(group => group.Rules).ToList();
    }
}

internal sealed record RobotsGroup(List<string> Agents, List<RobotsRule> Rules);

internal sealed record RobotsRule(bool Allow, string Pattern)
{
    public bool Matches(string path)
    {
        var anchored = Pattern.EndsWith('$');
        var pattern = anchored ? Pattern[..^1] : Pattern;
        return Matches(path, 0, pattern, 0, anchored);
    }

    private static bool Matches(string path, int p, string pattern, int q, bool anchored)
    {
        while (q < pattern.Length && pattern[q] != '*')
        {
            if (p >= path.Length || path[p] != pattern[q])
            {
                return false;
            }

            p++;
            q++;
        }

        if (q == pattern.Length)
        {
            return !anchored || p == path.Length;
        }

        // pattern[q] is '*': try every possible length for it.
        for (var skip = p; skip <= path.Length; skip++)
        {
            if (Matches(path, skip, pattern, q + 1, anchored))
            {
                return true;
            }
        }

        return false;
    }
}
