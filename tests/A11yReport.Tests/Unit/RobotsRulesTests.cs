using A11yReport.Core.Robots;

namespace A11yReport.Tests.Unit;

public sealed class RobotsRulesTests
{
    private const string Token = "TurkiyeWebA11yReport";

    [Fact]
    public void EmptyFile_AllowsEverything() => Assert.True(RobotsRules.Parse("").IsAllowed(Token, "/"));

    [Fact]
    public void DisallowAllForEveryone_BlocksUs() =>
        Assert.False(RobotsRules.Parse("User-agent: *\nDisallow: /").IsAllowed(Token, "/"));

    [Fact]
    public void OwnGroup_WinsOverStarGroup()
    {
        var rules = RobotsRules.Parse("User-agent: *\nDisallow: /\n\nUser-agent: turkiyewebA11yreport\nAllow: /");

        Assert.True(rules.IsAllowed(Token, "/"));
    }

    [Fact]
    public void OtherBotsGroup_DoesNotApplyToUs() =>
        Assert.True(RobotsRules.Parse("User-agent: Googlebot\nDisallow: /").IsAllowed(Token, "/"));

    [Fact]
    public void LongestMatchWins_AndAllowWinsTie()
    {
        var rules = RobotsRules.Parse("User-agent: *\nDisallow: /\nAllow: /$\nDisallow: /a\nAllow: /a");

        Assert.True(rules.IsAllowed(Token, "/"));
        Assert.False(rules.IsAllowed(Token, "/other"));
        Assert.True(rules.IsAllowed(Token, "/a"));
    }

    [Fact]
    public void Wildcards_Match()
    {
        var rules = RobotsRules.Parse("User-agent: *\nDisallow: /*.php$");

        Assert.False(rules.IsAllowed(Token, "/index.php"));
        Assert.True(rules.IsAllowed(Token, "/index.php?x=1"));
        Assert.True(rules.IsAllowed(Token, "/"));
    }

    [Fact]
    public void CommentsAndEmptyDisallow_AreIgnored()
    {
        var rules = RobotsRules.Parse("# hello\nUser-agent: * # everyone\nDisallow:\n");

        Assert.True(rules.IsAllowed(Token, "/"));
    }

    [Fact]
    public void SeveralAgentsInOneGroup_ShareRules()
    {
        var rules = RobotsRules.Parse("User-agent: Googlebot\nUser-agent: TurkiyeWebA11yReport\nDisallow: /");

        Assert.False(rules.IsAllowed(Token, "/"));
    }
}
