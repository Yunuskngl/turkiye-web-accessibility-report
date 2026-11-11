using A11yReport.Site.Html;
using Markdig;

namespace A11yReport.Site.Pages;

/// <summary>Renders docs/methodology.md, so the repository and the site always show the same text.</summary>
public static class MethodologyPage
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .DisableHtml()
        .Build();

    public static string Render(string markdown, string repositoryUrl)
    {
        var body = Markdown.ToHtml(markdown, Pipeline);
        return Layout.Page("Methodology", SitePage.Methodology, $"<div class=\"prose\">{body}</div>", repositoryUrl);
    }
}
