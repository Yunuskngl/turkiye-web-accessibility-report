namespace A11yReport.Core.Robots;

internal static class RobotsParser
{
    private const string UserAgentKey = "user-agent";
    private const string AllowKey = "allow";
    private const string DisallowKey = "disallow";

    public static List<RobotsGroup> ParseGroups(string text)
    {
        var groups = new List<RobotsGroup>();
        RobotsGroup? current = null;
        var lastWasAgent = false;

        foreach (var (key, value) in ReadLines(text))
        {
            if (key == UserAgentKey)
            {
                if (current is null || !lastWasAgent)
                {
                    current = new RobotsGroup([], []);
                    groups.Add(current);
                }

                current.Agents.Add(value);
                lastWasAgent = true;
                continue;
            }

            lastWasAgent = false;
            AddRule(current, key, value);
        }

        return groups;
    }

    private static void AddRule(RobotsGroup? group, string key, string value)
    {
        // Rules before any user-agent line, unknown keys and empty patterns are ignored (RFC 9309).
        if (group is null || value.Length == 0 || (key != AllowKey && key != DisallowKey))
        {
            return;
        }

        group.Rules.Add(new RobotsRule(key == AllowKey, value));
    }

    private static IEnumerable<(string Key, string Value)> ReadLines(string text)
    {
        foreach (var rawLine in text.Split('\n'))
        {
            var line = StripComment(rawLine).Trim();
            var colon = line.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0)
            {
                continue;
            }

            yield return (line[..colon].Trim().ToLowerInvariant(), line[(colon + 1)..].Trim());
        }
    }

    private static string StripComment(string line)
    {
        var hash = line.IndexOf('#', StringComparison.Ordinal);
        return hash >= 0 ? line[..hash] : line;
    }
}
