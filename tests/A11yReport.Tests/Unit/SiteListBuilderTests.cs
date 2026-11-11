using A11yReport.Core.Sites;

namespace A11yReport.Tests.Unit;

public sealed class SiteListBuilderTests
{
    private static readonly HashSet<string> None = [];

    [Fact]
    public void Build_KeepsTurkishDomains_AndIncludedOthers()
    {
        var origins = Origins(("https://www.a.com.tr", 1000), ("https://www.global.com", 1000), ("https://www.turk.com", 1000));

        var sites = SiteListBuilder.Build(origins, new HashSet<string> { "turk.com" }, None, 10).Sites;

        Assert.Equal(["a.com.tr", "turk.com"], sites.Select(s => s.Domain).Order());
    }

    [Fact]
    public void Build_OneEntryPerDomain_WithBestBucketAndHomeOrigin()
    {
        var origins = Origins(("https://obs.uni.edu.tr", 1000), ("https://www.uni.edu.tr", 5000), ("http://uni.edu.tr", 10000));

        var site = Assert.Single(SiteListBuilder.Build(origins, None, None, 10).Sites);

        Assert.Equal(1000, site.RankBucket);
        Assert.Equal("https://www.uni.edu.tr/", site.Url.ToString());
    }

    [Fact]
    public void Build_OnlySubdomainOrigins_IsLeftOutAndReported()
    {
        var origins = Origins(("https://portal.kurum.gov.tr", 1000), ("https://www.a.com.tr", 5000));

        var result = SiteListBuilder.Build(origins, None, None, 10);

        Assert.Equal("a.com.tr", Assert.Single(result.Sites).Domain);
        Assert.Equal(["kurum.gov.tr"], result.SubdomainOnly);
    }

    [Fact]
    public void Build_ExcludesOptOut_AndRespectsLimit_BestBucketFirst()
    {
        var origins = Origins(("https://www.a.com.tr", 5000), ("https://www.b.com.tr", 1000), ("https://www.c.com.tr", 1000));

        var sites = SiteListBuilder.Build(origins, None, new HashSet<string> { "c.com.tr" }, 1).Sites;

        Assert.Equal("b.com.tr", Assert.Single(sites).Domain);
    }

    [Fact]
    public void Build_SameInput_SameOrder()
    {
        var origins = Enumerable.Range(0, 50).Select(i => new CruxOrigin(new Uri($"https://www.site{i}.com.tr"), 1000)).ToList();

        var first = SiteListBuilder.Build(origins, None, None, 50).Sites.Select(s => s.Domain);
        var second = SiteListBuilder.Build(origins.AsEnumerable().Reverse(), None, None, 50).Sites.Select(s => s.Domain);

        Assert.Equal(first, second);
    }

    [Fact]
    public void CruxReader_Parse_ReadsOriginsAndSkipsBadRows()
    {
        var origins = CruxReader.Parse("origin,rank\nhttps://www.a.com.tr,1000\nnot a url,1000\nhttps://b.com.tr,x\n");

        Assert.Equal(new CruxOrigin(new Uri("https://www.a.com.tr"), 1000), Assert.Single(origins));
    }

    private static List<CruxOrigin> Origins(params (string Url, int Bucket)[] items) =>
        items.Select(item => new CruxOrigin(new Uri(item.Url), item.Bucket)).ToList();
}
