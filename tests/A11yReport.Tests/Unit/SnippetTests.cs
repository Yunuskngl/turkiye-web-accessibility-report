using A11yReport.Core.Results;

namespace A11yReport.Tests.Unit;

public sealed class SnippetTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Truncate_EmptyInput_ReturnsEmpty(string? html) => Assert.Equal(string.Empty, Snippet.Truncate(html));

    [Fact]
    public void Truncate_ShortInput_CollapsesWhitespace()
    {
        Assert.Equal("<a href=\"/\"> </a>", Snippet.Truncate("  <a href=\"/\">\n\t  </a>  "));
    }

    [Fact]
    public void Truncate_LongInput_CutsToMaxLengthWithEllipsis()
    {
        var result = Snippet.Truncate(new string('x', 1000));

        Assert.Equal(Snippet.MaxLength, result.Length);
        Assert.EndsWith("…", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Truncate_InputOfExactlyMaxLength_IsUnchanged()
    {
        var html = new string('x', Snippet.MaxLength);

        Assert.Equal(html, Snippet.Truncate(html));
    }
}
