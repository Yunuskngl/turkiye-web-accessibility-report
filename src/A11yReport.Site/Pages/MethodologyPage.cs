using A11yReport.Site.Html;
using Markdig;

namespace A11yReport.Site.Pages;

/// <summary>Renders docs/methodology.md (or its Turkish version), so the repository and the site show the same text.</summary>
public static class MethodologyPage
{
    // Generic attributes allow a fixed id such as {#opting-out} on a translated heading.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoIdentifiers()
        .UseGenericAttributes()
        .DisableHtml()
        .Build();

    public static string Render(SiteTexts t, string markdown, string repositoryUrl)
    {
        var body = Markdown.ToHtml(markdown, Pipeline);
        return Layout.Page(t, t.NavMethodology, SitePage.Methodology, $"<div class=\"prose\">{body}</div>", repositoryUrl);
    }
}
