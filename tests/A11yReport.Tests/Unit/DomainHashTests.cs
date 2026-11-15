using A11yReport.Core.Sites;

namespace A11yReport.Tests.Unit;

public sealed class DomainHashTests
{
    [Fact]
    public void Of_IsLowercaseSha256_AndIgnoresCaseAndSpaces()
    {
        // Expected value from: printf 'example.com.tr' | shasum -a 256
        const string Expected = "28b050da874df7d2c4ef8c2b6bfe1706e2b060a3938d115aaef37eb8ce0b01fd";

        Assert.Equal(Expected, DomainHash.Of("example.com.tr"));
        Assert.Equal(Expected, DomainHash.Of(" Example.COM.tr "));
    }

    [Fact]
    public void Hidden_ReturnsOnlyDomainsWhoseHashIsListed()
    {
        var hashes = new HashSet<string> { DomainHash.Of("person-name.com.tr") };

        var hidden = DomainHash.Hidden(["shop.com.tr", "person-name.com.tr"], hashes);

        Assert.Equal(["person-name.com.tr"], hidden);
    }

    [Fact]
    public void LoadHashes_ReadsTheSha256Column()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "sha256,reason\nABC123,test\n");

            Assert.Equal(["abc123"], SiteListFiles.LoadHashes(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
