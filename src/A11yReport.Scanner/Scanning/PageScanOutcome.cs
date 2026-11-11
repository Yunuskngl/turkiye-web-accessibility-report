using A11yReport.Core.Results;

namespace A11yReport.Scanner.Scanning;

public sealed record PageScanOutcome(
    ScanStatus Status,
    string Message,
    RequestCounts Requests,
    int? HttpStatus = null,
    PageScanResult? Result = null);
