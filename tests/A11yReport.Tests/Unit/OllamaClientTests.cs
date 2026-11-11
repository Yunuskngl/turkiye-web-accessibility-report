using System.Net;
using System.Text;
using System.Text.Json;
using A11yReport.Core.Explanations;
using A11yReport.Scanner.Explanations;
using Microsoft.Extensions.Options;

namespace A11yReport.Tests.Unit;

/// <summary>Tests the Ollama client against a fake HTTP handler, so no model or network is needed.</summary>
public sealed class OllamaClientTests
{
    private static readonly LlmOptions Options = new()
    {
        Provider = LlmOptions.OllamaProvider,
        BaseUrl = "http://127.0.0.1:11434/",
        Model = "test-model",
        TimeoutSeconds = 5,
        Temperature = 0.2,
        ExplanationsFile = "unused.json",
    };

    [Fact]
    public async Task GenerateJsonAsync_SendsModelPromptAndSchema_ReturnsResponseText()
    {
        var handler = new FakeHandler(HttpStatusCode.OK, """{"response":"{\"title\":\"t\"}"}""");
        using var http = new HttpClient(handler);

        var answer = await new OllamaClient(http, Microsoft.Extensions.Options.Options.Create(Options))
            .GenerateJsonAsync("the prompt", ExplanationPrompt.JsonSchema, CancellationToken.None);

        Assert.Equal("""{"title":"t"}""", answer);
        Assert.Equal("http://127.0.0.1:11434/api/generate", handler.RequestUri!.ToString());
        using var body = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("test-model", body.RootElement.GetProperty("model").GetString());
        Assert.Equal("the prompt", body.RootElement.GetProperty("prompt").GetString());
        Assert.False(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal(JsonValueKind.Object, body.RootElement.GetProperty("format").ValueKind);
    }

    [Fact]
    public async Task GenerateJsonAsync_ServerDown_ThrowsFriendlyError()
    {
        using var http = new HttpClient(new FakeHandler(HttpStatusCode.ServiceUnavailable, "{}"));
        var client = new OllamaClient(http, Microsoft.Extensions.Options.Options.Create(Options));

        var error = await Assert.ThrowsAsync<LlmException>(() =>
            client.GenerateJsonAsync("p", ExplanationPrompt.JsonSchema, CancellationToken.None));

        Assert.Contains("ollama serve", error.Message, StringComparison.Ordinal);
    }

    private sealed class FakeHandler(HttpStatusCode status, string responseBody) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status) { Content = new StringContent(responseBody, Encoding.UTF8, "application/json") };
        }
    }
}
