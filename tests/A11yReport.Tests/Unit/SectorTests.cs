using A11yReport.Core.Sites;

namespace A11yReport.Tests.Unit;

public sealed class SectorTests
{
    [Theory]
    [InlineData("turkiye.gov.tr", Sectors.Public)]
    [InlineData("ibb.bel.tr", Sectors.Public)]
    [InlineData("egm.pol.tr", Sectors.Public)]
    [InlineData("istanbul.edu.tr", Sectors.University)]
    [InlineData("shop.com.tr", null)]
    [InlineData("okul.k12.tr", null)]
    public void FromRules(string domain, string? expected) => Assert.Equal(expected, Sectors.FromRules(domain));

    [Fact]
    public void AddMissing_KeepsReviewedRows_AndAddsNewOnes()
    {
        SectorAssignment[] existing = [new("bank.com.tr", Sectors.Finance, "reviewed")];

        var result = SectorAssigner.AddMissing(existing, ["bank.com.tr", "kurum.gov.tr", "shop.com.tr"]);

        Assert.Equal(3, result.Count);
        Assert.Equal(existing[0], result[0]);
        Assert.Equal(new SectorAssignment("kurum.gov.tr", Sectors.Public, "auto: domain rule"), result[1]);
        Assert.Equal(new SectorAssignment("shop.com.tr", Sectors.Other, SiteListFiles.NeedsReviewNote), result[2]);
    }

    [Fact]
    public void SectorOf_ManualAssignmentWinsOverRule()
    {
        SectorAssignment[] assignments = [new("ibb.istanbul", Sectors.Public, "municipality")];

        Assert.Equal(Sectors.Public, SectorAssigner.SectorOf("ibb.istanbul", assignments));
        Assert.Equal(Sectors.Other, SectorAssigner.SectorOf("unknown.com.tr", assignments));
    }

    [Fact]
    public void SectorOf_UnknownSectorName_FallsBackToRule()
    {
        SectorAssignment[] assignments = [new("kurum.gov.tr", "typo", "")];

        Assert.Equal(Sectors.Public, SectorAssigner.SectorOf("kurum.gov.tr", assignments));
    }
}
