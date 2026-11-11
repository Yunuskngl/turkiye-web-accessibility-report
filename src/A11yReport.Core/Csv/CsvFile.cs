using System.Text;

namespace A11yReport.Core.Csv;

/// <summary>
/// Minimal RFC 4180 CSV reading and writing: comma separated, double quotes for fields
/// that contain commas, quotes or line breaks. Enough for our small hand-edited data files.
/// </summary>
public static class CsvFile
{
    private const char Separator = ',';
    private const char Quote = '"';

    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Read(string path)
    {
        var rows = Parse(File.ReadAllText(path, Encoding.UTF8));
        if (rows.Count == 0)
        {
            return [];
        }

        var header = rows[0].Select(name => name.Trim()).ToList();
        return rows.Skip(1).Where(row => !IsBlank(row)).Select(row => ToRecord(header, row)).ToList();
    }

    public static void Write(string path, IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string>> rows)
    {
        var text = new StringBuilder();
        text.AppendLine(string.Join(Separator, header.Select(Escape)));
        foreach (var row in rows)
        {
            text.AppendLine(string.Join(Separator, row.Select(Escape)));
        }

        File.WriteAllText(path, text.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public static List<List<string>> Parse(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                inQuotes = ReadQuoted(text, ref i, field);
                continue;
            }

            switch (c)
            {
                case Quote: inQuotes = true; break;
                case Separator: row.Add(field.ToString()); field.Clear(); break;
                case '\r': break;
                case '\n': row.Add(field.ToString()); field.Clear(); rows.Add(row); row = []; break;
                default: field.Append(c); break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }

    /// <summary>Handles one character inside quotes. Returns false when the quoted field ends.</summary>
    private static bool ReadQuoted(string text, ref int i, StringBuilder field)
    {
        if (text[i] != Quote)
        {
            field.Append(text[i]);
            return true;
        }

        var isEscapedQuote = i + 1 < text.Length && text[i + 1] == Quote;
        if (!isEscapedQuote)
        {
            return false;
        }

        field.Append(Quote);
        i++;
        return true;
    }

    private static string Escape(string value)
    {
        var needsQuotes = value.IndexOfAny([Separator, Quote, '\n', '\r']) >= 0;
        return needsQuotes ? $"{Quote}{value.Replace("\"", "\"\"", StringComparison.Ordinal)}{Quote}" : value;
    }

    private static bool IsBlank(List<string> row) => row.All(string.IsNullOrWhiteSpace);

    private static Dictionary<string, string> ToRecord(List<string> header, List<string> row)
    {
        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count; i++)
        {
            record[header[i]] = i < row.Count ? row[i].Trim() : string.Empty;
        }

        return record;
    }
}
