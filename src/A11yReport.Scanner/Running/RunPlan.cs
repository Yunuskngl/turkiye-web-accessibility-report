using A11yReport.Core.Sites;

namespace A11yReport.Scanner.Running;

public sealed record RunPlan(
    string Name,
    IReadOnlyList<SiteEntry> Sites,
    IReadOnlySet<string> OptOut,
    string ListSource,
    string ListSha256);
