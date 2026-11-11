using A11yReport.Core.Explanations;

namespace A11yReport.Tests.Unit;

public sealed class ExplanationTests
{
    private static readonly RuleInfo ImageAlt = new("image-alt", "Images must have alternative text",
        "Ensures <img> elements have alternative text", ["1.1.1"]);

    private const string ValidAnswer = """{"title":"Alt metni olmayan görseller","explanation":"Açıklama.","fixExample":"Hatalı: <img>\nDoğru: <img alt=\"x\">"}""";

    [Fact]
    public void Parse_ValidAnswer_ReturnsText()
    {
        var text = ExplanationParser.Parse(ValidAnswer);

        Assert.Equal("Alt metni olmayan görseller", text!.Title);
        Assert.StartsWith("Hatalı:", text.FixExample, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("""{"title":"x","explanation":"y"}""")]
    [InlineData("""{"title":"","explanation":"y","fixExample":"z"}""")]
    [InlineData("""{"title":1,"explanation":"y","fixExample":"z"}""")]
    public void Parse_InvalidAnswer_ReturnsNull(string answer) => Assert.Null(ExplanationParser.Parse(answer));

    [Theory]
    [InlineData("Hatalı: <a></a>\nDoğru: <a></a>")]
    [InlineData("Hatalı:")]
    [InlineData("Doğru: <a>x</a>\nHatalı: <a></a>")]
    [InlineData("<a>x</a>")]
    public void FixExample_SameCutOffOrUnmarked_IsRejected(string fixExample) =>
        Assert.False(ExplanationParser.HasDifferentWrongAndRightCode(fixExample));

    [Fact]
    public void FixExample_DifferentWrongAndRight_IsAccepted() =>
        Assert.True(ExplanationParser.HasDifferentWrongAndRightCode("Hatalı: <a href=\"/\"></a>\nDoğru: <a href=\"/\">Ana sayfa</a>"));

    [Fact]
    public void Parse_TooLongExplanation_ReturnsNull()
    {
        var answer = $$"""{"title":"x","explanation":"{{new string('a', ExplanationParser.MaxTextLength + 1)}}","fixExample":"z"}""";

        Assert.Null(ExplanationParser.Parse(answer));
    }

    [Fact]
    public void Prompt_ContainsOnlyAxeRuleMetadata()
    {
        var prompt = ExplanationPrompt.Build(ImageAlt);

        Assert.Contains("Rule id: image-alt", prompt, StringComparison.Ordinal);
        Assert.Contains("Images must have alternative text", prompt, StringComparison.Ordinal);
        Assert.Contains("WCAG success criteria: 1.1.1", prompt, StringComparison.Ordinal);
        Assert.Contains("Do not invent WCAG numbers, laws", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generator_SkipsExistingRules_AndSavesEachDraft()
    {
        var llm = new FakeLlm(ValidAnswer);
        var existing = new List<RuleExplanation> { Explanation("color-contrast", ExplanationStatus.Approved) };
        var saves = 0;

        var summary = await new ExplanationGenerator(llm, TimeProvider.System).GenerateMissingAsync(
            [ImageAlt, new RuleInfo("color-contrast", "h", "d", [])], existing, _ => saves++, CancellationToken.None);

        Assert.Equal(["image-alt"], llm.RuleIdsAsked);
        Assert.Equal(1, summary.Generated);
        Assert.Equal(1, saves);
        var draft = existing.Single(e => e.RuleId == "image-alt");
        Assert.Equal(ExplanationStatus.Draft, draft.Status);
        Assert.Equal("fake-model", draft.Model);
        Assert.Equal("1.1.1", draft.Criteria);
    }

    [Fact]
    public async Task Generator_UnusableAnswer_IsReportedNotStored()
    {
        var explanations = new List<RuleExplanation>();

        var summary = await new ExplanationGenerator(new FakeLlm("nonsense"), TimeProvider.System)
            .GenerateMissingAsync([ImageAlt], explanations, _ => { }, CancellationToken.None);

        Assert.Equal(["image-alt"], summary.Failed);
        Assert.Empty(explanations);
    }

    [Fact]
    public void File_RoundTrip_KeepsStatusAndDoesNotWriteComputedFields()
    {
        var path = Path.Combine(Path.GetTempPath(), $"explanations-{Guid.NewGuid():N}.json");
        try
        {
            ExplanationFile.Save(path, [Explanation("image-alt", ExplanationStatus.Approved)]);

            Assert.True(ExplanationFile.Load(path).Single().IsApproved);
            Assert.DoesNotContain("isApproved", File.ReadAllText(path), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void File_Missing_LoadsEmpty() => Assert.Empty(ExplanationFile.Load(Path.Combine(Path.GetTempPath(), "does-not-exist.json")));

    internal static RuleExplanation Explanation(string ruleId, string status, string title = "Başlık") =>
        new(ruleId, "help", "1.1.1", title, "Açıklama.", "Hatalı: a\nDoğru: b", status, "fake-model", DateTimeOffset.UnixEpoch);

    private sealed class FakeLlm(string answer) : ILlmClient
    {
        public List<string> RuleIdsAsked { get; } = [];

        public string ModelName => "fake-model";

        public Task<string> GenerateJsonAsync(string prompt, string jsonSchema, CancellationToken cancellationToken)
        {
            var line = prompt.Split('\n').First(l => l.Trim().StartsWith("Rule id:", StringComparison.Ordinal));
            RuleIdsAsked.Add(line.Split(':')[1].Trim());
            return Task.FromResult(answer);
        }
    }
}
