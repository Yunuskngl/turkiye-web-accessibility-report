namespace A11yReport.Scanner.Explanations;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";
    public const string OllamaProvider = "Ollama";

    /// <summary>Only "Ollama" is implemented. Other providers would add their own ILlmClient.</summary>
    public string Provider { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
    public double Temperature { get; set; }
    public string ExplanationsFile { get; set; } = string.Empty;

    public bool IsValid() =>
        Provider == OllamaProvider
        && Uri.TryCreate(BaseUrl, UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(Model)
        && TimeoutSeconds > 0
        && Temperature is >= 0 and <= 1
        && !string.IsNullOrWhiteSpace(ExplanationsFile);
}
