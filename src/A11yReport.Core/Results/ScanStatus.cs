namespace A11yReport.Core.Results;

/// <summary>Final state of one site in one run. Only <see cref="Done"/> has accessibility results.</summary>
public enum ScanStatus
{
    Done,
    SkippedOptOut,
    SkippedRobots,
    Unreachable,
    Timeout,
    DnsError,
    TlsError,
    NetworkError,
    HttpError,
    NonHtml,
    Duplicate,
    Failed,
}

public static class ScanStatusRules
{
    /// <summary>Transient errors where no page was received. Blocking responses are never retried.</summary>
    public static bool IsRetryable(ScanStatus status) =>
        status is ScanStatus.Timeout or ScanStatus.DnsError or ScanStatus.NetworkError;
}
