using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using A11yReport.Core.Explanations;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner.Explanations;

/// <summary>Calls a local Ollama server (/api/generate) with structured JSON output. No API key needed.</summary>
public sealed class OllamaClient(HttpClient http, IOptions<LlmOptions> options) : ILlmClient
{
    private const string GeneratePath = "api/generate";
    private readonly LlmOptions _options = options.Value;

    public string ModelName => _options.Model;

    public async Task<string> GenerateJsonAsync(string prompt, string jsonSchema, CancellationToken cancellationToken)
    {
        using var schema = JsonDocument.Parse(jsonSchema);
        var request = new GenerateRequest(_options.Model, prompt, Stream: false, schema.RootElement.Clone(),
            new GenerateOptions(_options.Temperature));
        try
        {
            using var response = await http.PostAsJsonAsync(new Uri(new Uri(_options.BaseUrl), GeneratePath), request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<GenerateResponse>(cancellationToken);
            return body?.Response ?? string.Empty;
        }
        catch (HttpRequestException exception)
        {
            throw new LlmException($"Ollama did not answer at {_options.BaseUrl}. Start it with: ollama serve", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new LlmException($"Ollama did not answer within {_options.TimeoutSeconds} seconds.", exception);
        }
    }

    private sealed record GenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] JsonElement Format,
        [property: JsonPropertyName("options")] GenerateOptions Options);

    private sealed record GenerateOptions([property: JsonPropertyName("temperature")] double Temperature);

    private sealed record GenerateResponse([property: JsonPropertyName("response")] string? Response);
}
