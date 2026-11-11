using A11yReport.Core.Results;

namespace A11yReport.Scanner.Scanning;

/// <summary>Maps HTTP statuses and Chromium network errors to our failure categories.</summary>
public static class FailureClassifier
{
    // Responses that mean "the site does not want this request now". We stop and do not retry.
    private static readonly int[] BlockingStatuses = [401, 403, 429, 503];

    private static readonly (string Code, ScanStatus Status)[] ChromiumErrors =
    [
        ("ERR_NAME_NOT_RESOLVED", ScanStatus.DnsError),
        ("ERR_NAME_RESOLUTION_FAILED", ScanStatus.DnsError),
        ("ERR_CERT_", ScanStatus.TlsError),
        ("ERR_SSL_", ScanStatus.TlsError),
        ("ERR_BAD_SSL", ScanStatus.TlsError),
        ("ERR_CONNECTION_REFUSED", ScanStatus.Unreachable),
        ("ERR_CONNECTION_RESET", ScanStatus.Unreachable),
        ("ERR_CONNECTION_CLOSED", ScanStatus.Unreachable),
        ("ERR_EMPTY_RESPONSE", ScanStatus.Unreachable),
        ("ERR_ADDRESS_UNREACHABLE", ScanStatus.Unreachable),
        ("ERR_CONNECTION_TIMED_OUT", ScanStatus.Timeout),
        ("ERR_TIMED_OUT", ScanStatus.Timeout),
        ("ERR_TOO_MANY_REDIRECTS", ScanStatus.HttpError),
    ];

    public static bool IsBlocking(int httpStatus) => BlockingStatuses.Contains(httpStatus);

    public static ScanStatus FromHttpStatus(int httpStatus) =>
        IsBlocking(httpStatus) ? ScanStatus.Unreachable : ScanStatus.HttpError;

    public static ScanStatus FromChromiumMessage(string message)
    {
        foreach (var (code, status) in ChromiumErrors)
        {
            if (message.Contains(code, StringComparison.Ordinal))
            {
                return status;
            }
        }

        return ScanStatus.NetworkError;
    }

    public static ScanStatus FromHttpRequestError(System.Net.Http.HttpRequestError error) => error switch
    {
        System.Net.Http.HttpRequestError.NameResolutionError => ScanStatus.DnsError,
        System.Net.Http.HttpRequestError.SecureConnectionError => ScanStatus.TlsError,
        System.Net.Http.HttpRequestError.ConnectionError => ScanStatus.Unreachable,
        _ => ScanStatus.NetworkError,
    };

    public static string Describe(ScanStatus status, int? httpStatus = null) => status switch
    {
        ScanStatus.Unreachable when httpStatus is not null =>
            $"The site refused or limited the request (HTTP {httpStatus}). Recorded as unreachable and not retried.",
        ScanStatus.Unreachable => "The site refused or dropped the connection. Recorded as unreachable and not retried.",
        ScanStatus.HttpError => $"The server answered with HTTP {httpStatus}.",
        ScanStatus.Timeout => "The page did not finish loading in time.",
        ScanStatus.DnsError => "The domain name could not be resolved (DNS).",
        ScanStatus.TlsError => "The secure connection (TLS certificate) failed.",
        ScanStatus.NonHtml => "The address did not return an HTML page.",
        _ => "The page could not be loaded (network error).",
    };
}
