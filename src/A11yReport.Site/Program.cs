using System.Text.RegularExpressions;
using A11yReport.Core.Explanations;
using A11yReport.Core.Report;
using A11yReport.Core.Sites;
using A11yReport.Site;
using A11yReport.Site.Html;
using A11yReport.Site.Lookup;
using A11yReport.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

const int Success = 0;
const int Failure = 1;
const int UsageError = 2;
const string Usage = """
    Usage (run from the repository root):
      dotnet run --project src/A11yReport.Site -- --run <name> [--pilot]
    """;

var parsed = args switch
{
    ["--run", var name] => (Name: name, IsPilot: false),
    ["--run", var name, "--pilot"] => (Name: name, IsPilot: true),
    _ => (Name: string.Empty, IsPilot: false),
};

if (!Regex.IsMatch(parsed.Name, "^[a-z0-9][a-z0-9-]{0,63}$"))
{
    Console.Error.WriteLine(Usage);
    return UsageError;
}

using var host = BuildHost();
var reader = host.Services.GetRequiredService<ReportReader>();
var input = await reader.ReadAsync(parsed.Name, CancellationToken.None);
if (input is null)
{
    Console.Error.WriteLine($"Run '{parsed.Name}' was not found in the database.");
    return Failure;
}

var siteOptions = host.Services.GetRequiredService<IOptions<SiteOptions>>().Value;
var optOut = SiteListFiles.LoadDomains(siteOptions.OptOutFile);
var data = ReportBuilder.Build(input.Value.Run, input.Value.Sites, optOut);
var ruleNames = RuleNames.From(ExplanationFile.Load(siteOptions.ExplanationsFile));
var lookupSites = LookupBuilder.Build(input.Value.Run, input.Value.Sites, optOut, ruleNames);
var folder = ActivatorUtilities.CreateInstance<SiteGenerator>(host.Services).Generate(data, lookupSites, ruleNames, parsed.IsPilot);
Console.WriteLine($"Site for run {parsed.Name} written to {folder} ({data.Coverage.Scanned} pages checked).");
return Success;

static IHost BuildHost()
{
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = [],
        ContentRootPath = AppContext.BaseDirectory,
    });
    builder.Services.AddOptions<SiteOptions>()
        .Bind(builder.Configuration.GetSection(SiteOptions.SectionName))
        .Validate(options => options.IsValid(), "The Report section in appsettings.json is incomplete.")
        .ValidateOnStart();
    builder.Services.AddOptions<StorageOptions>().Bind(builder.Configuration.GetSection(StorageOptions.SectionName));
    builder.Services.AddDbContextFactory<ReportDbContext>((services, options) =>
    {
        var path = Path.GetFullPath(services.GetRequiredService<IOptions<StorageOptions>>().Value.DatabasePath);
        options.UseSqlite(new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly }.ToString());
    });
    builder.Services.AddSingleton<ReportReader>();
    return builder.Build();
}
