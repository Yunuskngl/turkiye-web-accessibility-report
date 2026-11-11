using A11yReport.Core.Sites;

namespace A11yReport.Tests.Unit;

public sealed class DomainKeyTests
{
    [Theory]
    [InlineData("www.example.com.tr", "example.com.tr")]
    [InlineData("obs.istanbul.edu.tr", "istanbul.edu.tr")]
    [InlineData("giris.turkiye.gov.tr", "turkiye.gov.tr")]
    [InlineData("www.example.tr", "example.tr")]
    [InlineData("m.shop.example.tr", "example.tr")]
    [InlineData("WWW.Example.COM.", "example.com")]
    [InlineData("enstitu.ibb.istanbul", "ibb.istanbul")]
    public void FromHost_ReturnsRegistrableDomain(string host, string expected) =>
        Assert.Equal(expected, DomainKey.FromHost(host));

    [Theory]
    [InlineData("example.com.tr", true)]
    [InlineData("example.tr", true)]
    [InlineData("ibb.istanbul", true)]
    [InlineData("site.nc.tr", false)]
    [InlineData("example.com", false)]
    [InlineData("example.trade", false)]
    public void IsTurkishTld(string domain, bool expected) => Assert.Equal(expected, DomainKey.IsTurkishTld(domain));

    [Theory]
    [InlineData("example.com.tr", true)]
    [InlineData("www.example.com.tr", true)]
    [InlineData("obs.example.com.tr", false)]
    public void IsHomeHostOf(string host, bool expected) => Assert.Equal(expected, DomainKey.IsHomeHostOf(host, "example.com.tr"));
}
