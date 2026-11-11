using A11yReport.Core.Results;
using A11yReport.Core.Sites;

namespace A11yReport.Storage;

internal static class SiteScanMapper
{
    private const char TagSeparator = ' ';

    public static SiteScanEntity ToEntity(SiteScanResult result, int runId, int sessionId, int maxNodes)
    {
        var entity = new SiteScanEntity
        {
            RunId = runId,
            SessionId = sessionId,
            Domain = result.Domain,
            Sector = result.Sector,
            Url = result.Url.ToString(),
            Status = result.Status,
            Message = result.Message,
            Attempts = result.Attempts,
            StartedAt = result.StartedAt,
            FinishedAt = result.FinishedAt,
            HttpStatus = result.HttpStatus,
            FinalUrl = result.FinalUrl?.ToString(),
            FinalSiteKey = result.FinalUrl is null ? null : DomainKey.FromHost(result.FinalUrl.Host),
            AxeVersion = result.AxeVersion,
            RobotsRequests = result.Requests.Robots,
            DocumentRequests = result.Requests.Documents,
            SubresourceRequests = result.Requests.Subresources,
            Violations = ToViolations(result.Viewports ?? [], maxNodes),
        };
        CopyFacts(result.Facts, entity);
        return entity;
    }

    private static void CopyFacts(PageFacts? facts, SiteScanEntity entity)
    {
        if (facts is null)
        {
            return;
        }

        entity.Lang = facts.Lang;
        entity.HasTitle = facts.HasTitle;
        entity.LandmarkCount = facts.LandmarkCount;
        entity.HasMain = facts.HasMain;
        entity.HeadingCount = facts.HeadingCount;
        entity.H1Count = facts.H1Count;
        entity.SkipsHeadingLevel = facts.SkipsHeadingLevel;
    }

    private static List<ViolationEntity> ToViolations(IEnumerable<ViewportResult> viewports, int maxNodes) =>
        viewports.SelectMany(viewport => viewport.Violations.Select(violation => new ViolationEntity
        {
            Viewport = viewport.Name,
            RuleId = violation.RuleId,
            Impact = violation.Impact,
            Help = violation.Help,
            Tags = string.Join(TagSeparator, violation.Tags),
            NodeCount = violation.NodeCount,
            Nodes = violation.Nodes.Take(maxNodes)
                .Select(node => new ViolationNodeEntity { Selector = node.Selector, Snippet = node.Snippet })
                .ToList(),
        })).ToList();
}
