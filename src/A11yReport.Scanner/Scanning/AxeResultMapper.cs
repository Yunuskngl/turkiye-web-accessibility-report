using A11yReport.Core.Results;
using Deque.AxeCore.Commons;

namespace A11yReport.Scanner.Scanning;

public static class AxeResultMapper
{
    public static ViewportResult ToViewportResult(AxeResult result, ViewportOptions viewport)
    {
        var violations = (result.Violations ?? []).Select(ToViolation).ToList();
        return new ViewportResult(viewport.Name, viewport.Width, viewport.Height, violations);
    }

    private static RuleViolation ToViolation(AxeResultItem item)
    {
        var nodes = (item.Nodes ?? []).Select(ToNode).ToList();
        var tags = item.Tags ?? [];
        return new RuleViolation(item.Id, item.Impact ?? Impact.Unknown, item.Help ?? string.Empty, tags, nodes);
    }

    private static ViolationNode ToNode(AxeResultNode node) =>
        new(node.Target?.ToString() ?? string.Empty, Snippet.Truncate(node.Html));
}
