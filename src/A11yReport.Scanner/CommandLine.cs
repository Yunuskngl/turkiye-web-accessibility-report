using System.Globalization;
using System.Text.RegularExpressions;
using A11yReport.Core.Input;

namespace A11yReport.Scanner;

public abstract record CommandArgs;
public sealed record InstallBrowserArgs : CommandArgs;
public sealed record UrlArgs(Uri Url) : CommandArgs;
public sealed record BuildListArgs(string SnapshotPath) : CommandArgs;
public sealed record ScanArgs(string RunName, int? Limit) : CommandArgs;

public static partial class CommandLine
{
    public const string Usage = """
        Usage (run from the repository root):
          dotnet run --project src/A11yReport.Scanner -- install-browser
          dotnet run --project src/A11yReport.Scanner -- url <url>
          dotnet run --project src/A11yReport.Scanner -- build-list <crux-snapshot.csv.gz>
          dotnet run --project src/A11yReport.Scanner -- scan --run <name> [--limit <n>]

        Run names use lowercase letters, digits and dashes, for example 2026-pilot.
        """;

    /// <summary>Returns null when the arguments are not valid.</summary>
    public static CommandArgs? Parse(string[] args) => args switch
    {
        ["install-browser"] => new InstallBrowserArgs(),
        ["url", var text] => TargetUrl.TryParse(text, out var url) ? new UrlArgs(url) : null,
        ["build-list", var path] => new BuildListArgs(path),
        ["scan", "--run", var name] => ScanFor(name, null),
        ["scan", "--run", var name, "--limit", var limit] => ScanFor(name, limit),
        _ => null,
    };

    private static ScanArgs? ScanFor(string name, string? limitText)
    {
        if (!RunName().IsMatch(name))
        {
            return null;
        }

        if (limitText is null)
        {
            return new ScanArgs(name, null);
        }

        var valid = int.TryParse(limitText, NumberStyles.None, CultureInfo.InvariantCulture, out var limit) && limit > 0;
        return valid ? new ScanArgs(name, limit) : null;
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,63}$")]
    private static partial Regex RunName();
}
