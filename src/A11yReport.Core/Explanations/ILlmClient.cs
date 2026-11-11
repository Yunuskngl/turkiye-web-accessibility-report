namespace A11yReport.Core.Explanations;

/// <summary>
/// Provider-neutral text generation (Ollama by default). Callers must only send rule metadata from
/// axe-core: page HTML or page text is untrusted input and is never passed to a language model.
/// </summary>
public interface ILlmClient
{
    /// <summary>The model name, recorded with every generated explanation.</summary>
    string ModelName { get; }

    /// <summary>Generates one JSON object that matches <paramref name="jsonSchema"/>.</summary>
    Task<string> GenerateJsonAsync(string prompt, string jsonSchema, CancellationToken cancellationToken);
}
