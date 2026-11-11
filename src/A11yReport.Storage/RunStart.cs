using A11yReport.Core.Results;

namespace A11yReport.Storage;

public sealed record RunStart(
    string Name,
    string ListSource,
    string ListSha256,
    ToolVersions Versions,
    string UserAgent,
    string SettingsJson);

public sealed record RunHandle(int RunId, int SessionId, bool IsResume);

/// <summary>A run that cannot start; the message is safe to show to people.</summary>
public sealed class RunStartException(string message) : Exception(message);
