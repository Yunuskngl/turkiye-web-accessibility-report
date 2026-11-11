using A11yReport.Core.Explanations;
using A11yReport.Scanner.Explanations;
using A11yReport.Scanner.Robots;
using A11yReport.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner;

/// <summary>Configuration, logging and services. Relative paths in settings resolve from the current folder.</summary>
public static class AppHost
{
    public static IHost Build()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Logging.AddConsole(console => console.LogToStandardErrorThreshold = LogLevel.Trace);
        AddOptions<ScanOptions>(builder, ScanOptions.SectionName, o => o.IsValid());
        AddOptions<RunOptions>(builder, RunOptions.SectionName, o => o.IsValid());
        AddOptions<DataOptions>(builder, DataOptions.SectionName, o => o.IsValid());
        AddOptions<StorageOptions>(builder, StorageOptions.SectionName, o => o.IsValid());
        AddOptions<LlmOptions>(builder, LlmOptions.SectionName, o => o.IsValid());

        builder.Services.AddDbContextFactory<ReportDbContext>(ConfigureDatabase);
        builder.Services.AddSingleton<RunStore>();
        builder.Services.AddSingleton<RobotsChecker>();
        builder.Services.AddSingleton<ReportReader>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<ExplanationGenerator>();
        builder.Services.AddSingleton<ILlmClient>(CreateLlmClient);
        return builder.Build();
    }

    private static void AddOptions<T>(HostApplicationBuilder builder, string section, Func<T, bool> isValid)
        where T : class
    {
        builder.Services.AddOptions<T>()
            .Bind(builder.Configuration.GetSection(section))
            .Validate(isValid, $"The {section} section in appsettings.json is incomplete.")
            .ValidateOnStart();
    }

    // One long-lived HttpClient for the local Ollama server; IHttpClientFactory would need an extra package.
    private static OllamaClient CreateLlmClient(IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<LlmOptions>>();
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds) };
        return new OllamaClient(http, options);
    }

    private static void ConfigureDatabase(IServiceProvider services, DbContextOptionsBuilder options)
    {
        var path = Path.GetFullPath(services.GetRequiredService<IOptions<StorageOptions>>().Value.DatabasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var connection = new SqliteConnectionStringBuilder { DataSource = path };
        options.UseSqlite(connection.ToString());
    }
}
