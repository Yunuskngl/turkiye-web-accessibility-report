using A11yReport.Core.Results;

namespace A11yReport.Tests.Unit;

public sealed class WcagCriteriaTests
{
    [Fact]
    public void FromTags_ConvertsCriterionTags_AndIgnoresLevelTags()
    {
        string[] tags = ["cat.color", "wcag2aa", "wcag143", "wcag21aa", "wcag1410", "wcag2411", "TTv5"];

        Assert.Equal(["1.4.3", "1.4.10", "2.4.11"], WcagCriteria.FromTags(tags));
    }

    [Fact]
    public void FromTags_NoCriteria_ReturnsEmpty() => Assert.Empty(WcagCriteria.FromTags(["wcag2a", "best-practice"]));
}
