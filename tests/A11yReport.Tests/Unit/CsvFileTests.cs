using A11yReport.Core.Csv;

namespace A11yReport.Tests.Unit;

public sealed class CsvFileTests
{
    [Fact]
    public void Parse_HandlesQuotesCommasAndLineBreaks()
    {
        var rows = CsvFile.Parse("a,b\n\"x, y\",\"say \"\"hi\"\"\"\n\"two\nlines\",z\n");

        Assert.Equal(["a", "b"], rows[0]);
        Assert.Equal(["x, y", "say \"hi\""], rows[1]);
        Assert.Equal(["two\nlines", "z"], rows[2]);
    }

    [Fact]
    public void WriteThenRead_RoundTripsValues()
    {
        var path = Path.GetTempFileName();
        try
        {
            CsvFile.Write(path, ["domain", "notes"], [["a.com.tr", "bank, reviewed"], ["b.gov.tr", ""]]);

            var rows = CsvFile.Read(path);

            Assert.Equal("bank, reviewed", rows[0]["notes"]);
            Assert.Equal("b.gov.tr", rows[1]["domain"]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
