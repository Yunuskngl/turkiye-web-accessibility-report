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

    public static string Render(SiteTexts t, string markdown, string repositoryUrl) =>
        RenderMarkdown(t, t.NavMethodology, SitePage.Methodology, markdown, repositoryUrl);

    /// <summary>Any Markdown page of the site (methodology, privacy notice) inside the shared frame.</summary>
    public static string RenderMarkdown(SiteTexts t, string title, SitePage page, string markdown, string repositoryUrl)
    {
        var body = Markdown.ToHtml(markdown, Pipeline);
        return Layout.Page(t, title, page, $"<div class=\"prose\">{body}</div>", repositoryUrl);
    }
}
