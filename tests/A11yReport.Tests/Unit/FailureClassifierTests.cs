using A11yReport.Core.Results;
using A11yReport.Scanner.Scanning;

namespace A11yReport.Tests.Unit;

public sealed class FailureClassifierTests
{
    [Theory]
    [InlineData(401, ScanStatus.Unreachable)]
    [InlineData(403, ScanStatus.Unreachable)]
    [InlineData(429, ScanStatus.Unreachable)]
    [InlineData(503, ScanStatus.Unreachable)]
    [InlineData(404, ScanStatus.HttpError)]
    [InlineData(500, ScanStatus.HttpError)]
    public void FromHttpStatus(int code, ScanStatus expected) => Assert.Equal(expected, FailureClassifier.FromHttpStatus(code));

    [Theory]
    [InlineData("net::ERR_NAME_NOT_RESOLVED at https://x.tr/", ScanStatus.DnsError)]
    [InlineData("net::ERR_CERT_DATE_INVALID at https://x.tr/", ScanStatus.TlsError)]
    [InlineData("net::ERR_CONNECTION_REFUSED at https://x.tr/", ScanStatus.Unreachable)]
    [InlineData("net::ERR_CONNECTION_TIMED_OUT at https://x.tr/", ScanStatus.Timeout)]
    [InlineData("something unexpected", ScanStatus.NetworkError)]
    public void FromChromiumMessage(string message, ScanStatus expected) =>
        Assert.Equal(expected, FailureClassifier.FromChromiumMessage(message));

    [Theory]
    [InlineData(ScanStatus.Timeout, false)]
    [InlineData(ScanStatus.NetworkError, true)]
    [InlineData(ScanStatus.DnsError, true)]
    [InlineData(ScanStatus.Unreachable, false)]
    [InlineData(ScanStatus.HttpError, false)]
    [InlineData(ScanStatus.TlsError, false)]
    public void OnlyTransientErrorsAreRetryable(ScanStatus status, bool expected) =>
        Assert.Equal(expected, ScanStatusRules.IsRetryable(status));
}
