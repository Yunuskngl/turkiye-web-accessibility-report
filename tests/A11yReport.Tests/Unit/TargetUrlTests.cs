using A11yReport.Core.Input;

namespace A11yReport.Tests.Unit;

public sealed class TargetUrlTests
{
    [Theory]
    [InlineData("https://example.com.tr/", "https://example.com.tr/")]
    [InlineData("http://example.com.tr", "http://example.com.tr/")]
    [InlineData("example.com.tr", "https://example.com.tr/")]
    [InlineData("  example.com.tr  ", "https://example.com.tr/")]
    public void TryParse_ValidInput_ReturnsUrl(string input, string expected)
    {
        Assert.True(TargetUrl.TryParse(input, out var url));
        Assert.Equal(expected, url.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ftp://example.com")]
    [InlineData("file:///etc/passwd")]
    [InlineData("https://")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input) => Assert.False(TargetUrl.TryParse(input, out _));
}
