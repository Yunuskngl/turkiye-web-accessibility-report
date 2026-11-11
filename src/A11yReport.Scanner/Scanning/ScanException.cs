using A11yReport.Core.Results;

namespace A11yReport.Scanner.Scanning;

/// <summary>A page scan that stopped with a known failure category. The message is safe to show to people.</summary>
public sealed class ScanException(ScanStatus status, string message, int? httpStatus = null, Exception? innerException = null)
    : Exception(message, innerException)
{
    public ScanStatus Status { get; } = status;
    public int? HttpStatus { get; } = httpStatus;
}
