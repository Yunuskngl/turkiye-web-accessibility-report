using A11yReport.Scanner;
using A11yReport.Scanner.Commands;
using Microsoft.Extensions.DependencyInjection;

var command = CommandLine.Parse(args);
if (command is null)
{
    Console.Error.WriteLine(CommandLine.Usage);
    return ExitCodes.Usage;
}

if (command is InstallBrowserArgs)
{
    // Playwright ships its own installer; this avoids needing PowerShell for playwright.ps1.
    return Microsoft.Playwright.Program.Main(["install", "chromium"]);
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

using var host = AppHost.Build();
var services = host.Services;
return command switch
{
    UrlArgs url => await ActivatorUtilities.CreateInstance<UrlCommand>(services).RunAsync(url.Url, cancellation.Token),
    BuildListArgs list => ActivatorUtilities.CreateInstance<BuildListCommand>(services).Run(list.SnapshotPath),
    ScanArgs scan => await ActivatorUtilities.CreateInstance<ScanRunCommand>(services).RunAsync(scan, cancellation.Token),
    ExplainArgs explain => await ActivatorUtilities.CreateInstance<ExplainCommand>(services).RunAsync(explain.RunName, cancellation.Token),
    _ => ExitCodes.Usage,
};
