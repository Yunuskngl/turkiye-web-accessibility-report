namespace A11yReport.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// How many example elements (selector + snippet) to keep per rule and viewport.
    /// The full element count is always kept; fewer stored snippets means less stored page markup.
    /// </summary>
    public int MaxNodesPerRule { get; set; }

    public bool IsValid() => !string.IsNullOrWhiteSpace(DatabasePath) && MaxNodesPerRule > 0;
}
