using A11yReport.Core.Explanations;
using A11yReport.Scanner.Explanations;
using A11yReport.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner.Commands;

/// <summary>
/// Drafts a Turkish explanation for every axe rule that failed in a run, once per rule. Only axe's
/// own rule metadata goes to the language model; nothing from the scanned pages.
/// </summary>
public sealed class ExplainCommand(
    ReportReader reader,
    ExplanationGenerator generator,
    IOptions<ScanOptions> scanOptions,
    IOptions<LlmOptions> llmOptions,
    ILogger<ExplainCommand> logger)
{
    private readonly string _file = llmOptions.Value.ExplanationsFile;

    public async Task<int> RunAsync(string runName, CancellationToken cancellationToken)
    {
        var ruleIds = await reader.ReadRuleIdsAsync(runName, cancellationToken);
        if (ruleIds.Count == 0)
        {
            await Console.Error.WriteLineAsync($"No failed rules found for run '{runName}'.");
            return ExitCodes.Usage;
        }

        await using var session = await BrowserStarter.TryStartAsync(logger);
        if (session is null)
        {
            return ExitCodes.ScanFailed;
        }

        var rules = await AxeRuleCatalog.ReadAsync(session.Browser, ruleIds, scanOptions.Value.AxeTags);
        return await GenerateAsync(rules, cancellationToken);
    }

    private async Task<int> GenerateAsync(IReadOnlyList<RuleInfo> rules, CancellationToken cancellationToken)
    {
        try
        {
            var explanations = ExplanationFile.Load(_file);
            var summary = await generator.GenerateMissingAsync(rules, explanations, all => ExplanationFile.Save(_file, all), cancellationToken);
            Console.WriteLine($"{rules.Count} rules: {summary.Existing} already in {_file}, {summary.Generated} new drafts.");
            if (summary.Failed.Count > 0)
            {
                Console.WriteLine($"No usable answer for: {string.Join(", ", summary.Failed)}. Run the command again to retry.");
            }

            Console.WriteLine("Review the drafts and set \"status\" to \"approved\" for the ones that may be published.");
            return ExitCodes.Success;
        }
        catch (LlmException exception)
        {
            Log.LlmFailed(logger, exception);
            await Console.Error.WriteLineAsync(exception.Message);
            return ExitCodes.ScanFailed;
        }
    }
}
