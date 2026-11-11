using System.Globalization;
using System.IO.Compression;
using A11yReport.Core.Csv;

namespace A11yReport.Core.Sites;

public static class CruxReader
{
    private const string OriginColumn = "origin";
    private const string RankColumn = "rank";

    /// <summary>Reads a CrUX top list snapshot (origin,rank), plain or .gz.</summary>
    public static IReadOnlyList<CruxOrigin> Read(string path)
    {
        using var file = File.OpenRead(path);
        using Stream stream = path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
            ? new GZipStream(file, CompressionMode.Decompress)
            : file;
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    public static IReadOnlyList<CruxOrigin> Parse(string csv)
    {
        var rows = CsvFile.Parse(csv);
        if (rows.Count == 0)
        {
            return [];
        }

        var header = rows[0];
        var originIndex = header.IndexOf(OriginColumn);
        var rankIndex = header.IndexOf(RankColumn);
        return rows.Skip(1).Select(row => ToOrigin(row, originIndex, rankIndex)).OfType<CruxOrigin>().ToList();
    }

    private static CruxOrigin? ToOrigin(List<string> row, int originIndex, int rankIndex)
    {
        if (row.Count <= Math.Max(originIndex, rankIndex))
        {
            return null;
        }

        var validOrigin = Uri.TryCreate(row[originIndex], UriKind.Absolute, out var origin);
        var validRank = int.TryParse(row[rankIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var rank);
        return validOrigin && validRank ? new CruxOrigin(origin!, rank) : null;
    }
}
