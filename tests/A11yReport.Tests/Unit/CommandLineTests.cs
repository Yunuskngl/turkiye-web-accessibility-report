using A11yReport.Scanner;

namespace A11yReport.Tests.Unit;

public sealed class CommandLineTests
{
    [Fact]
    public void Parse_ScanWithLimit() =>
        Assert.Equal(new ScanArgs("2026-pilot", 50), CommandLine.Parse(["scan", "--run", "2026-pilot", "--limit", "50"]));

    [Fact]
    public void Parse_Url() =>
        Assert.Equal(new UrlArgs(new Uri("https://example.com.tr/")), CommandLine.Parse(["url", "example.com.tr"]));

    [Theory]
    [InlineData("scan", "--run", "Bad Name")]
    [InlineData("scan", "--run", "../escape")]
    [InlineData("scan", "--limit", "5")]
    [InlineData("unknown")]
    public void Parse_Invalid_ReturnsNull(params string[] args) => Assert.Null(CommandLine.Parse(args));

    [Fact]
    public void Parse_NegativeLimit_ReturnsNull() =>
        Assert.Null(CommandLine.Parse(["scan", "--run", "pilot", "--limit", "-1"]));
}
