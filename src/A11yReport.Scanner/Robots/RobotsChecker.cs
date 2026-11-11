using System.Net;
using A11yReport.Core.Results;
using A11yReport.Core.Robots;
using A11yReport.Scanner.Scanning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace A11yReport.Scanner.Robots;

/// <summary>Result of the robots.txt check. <see cref="Stop"/> is null when we may load the page.</summary>
public sealed record RobotsCheck(ScanStatus? Stop, string Message, int Requests, int? HttpStatus = null);

/// <summary>Fetches robots.txt with our User-Agent and applies RFC 9309, with stricter handling of blocking responses.</summary>
public sealed class RobotsChecker : IDisposable
{
    private const int MaxRedirects = 5;          // RFC 9309 section 2.3.1.2
    private const int MaxBytes = 500 * 1024;     // RFC 9309 section 2.5
    private const string AllowedMessage = "Allowed by robots.txt.";

    private readonly HttpClient _http;
    private readonly ScanOptions _scan;
    private readonly ILogger<RobotsChecker> _logger;

    public RobotsChecker(IOptions<ScanOptions> scan, IOptions<RunOptions> run, ILogger<RobotsChecker> logger)
    {
        _scan = scan.Value;
        _logger = logger;
        var handler = new SocketsHttpHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(run.Value.RobotsTimeoutSeconds) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(_scan.UserAgent);
    }

    public async Task<RobotsCheck> CheckAsync(Uri pageUrl, CancellationToken cancellationToken)
    {
        var url = new Uri(pageUrl, "/robots.txt");
        for (var requests = 1; requests <= MaxRedirects + 1; requests++)
        {
            var (response, failure) = await SendAsync(url, requests, cancellationToken);
            if (failure is not null)
            {
                return failure;
            }

            using (response)
            {
                var location = RedirectTarget(response!, url);
                if (location is null)
                {
                    return await EvaluateAsync(response!, pageUrl, requests, cancellationToken);
                }

                url = location;
            }
        }

        // Too many redirects: RFC 9309 treats robots.txt as unavailable, which allows crawling.
        return new RobotsCheck(null, AllowedMessage, MaxRedirects + 1);
    }

    public void Dispose() => _http.Dispose();

    private async Task<(HttpResponseMessage? Response, RobotsCheck? Failure)> SendAsync(
        Uri url, int requests, CancellationToken cancellationToken)
    {
        try
        {
            return (await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken), null);
        }
        catch (HttpRequestException exception)
        {
            Log.RobotsFailed(_logger, exception, url);
            var status = FailureClassifier.FromHttpRequestError(exception.HttpRequestError);
            return (null, new RobotsCheck(status, FailureClassifier.Describe(status), requests));
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            Log.RobotsFailed(_logger, exception, url);
            return (null, new RobotsCheck(ScanStatus.Timeout, "robots.txt did not answer in time.", requests));
        }
    }

    private static Uri? RedirectTarget(HttpResponseMessage response, Uri current)
    {
        var code = (int)response.StatusCode;
        var isRedirect = code is >= 300 and < 400 && response.Headers.Location is not null;
        return isRedirect ? new Uri(current, response.Headers.Location!) : null;
    }

    private async Task<RobotsCheck> EvaluateAsync(
        HttpResponseMessage response, Uri pageUrl, int requests, CancellationToken cancellationToken)
    {
        var code = (int)response.StatusCode;
        if (FailureClassifier.IsBlocking(code))
        {
            return new RobotsCheck(ScanStatus.Unreachable, FailureClassifier.Describe(ScanStatus.Unreachable, code), requests, code);
        }

        if (code >= 500)
        {
            return new RobotsCheck(ScanStatus.SkippedRobots,
                $"robots.txt returned HTTP {code}; treated as disallow (RFC 9309).", requests, code);
        }

        if (code >= 400)
        {
            return new RobotsCheck(null, AllowedMessage, requests, code);
        }

        var rules = RobotsRules.Parse(await ReadLimitedAsync(response, cancellationToken));
        return rules.IsAllowed(_scan.RobotsToken, pageUrl.AbsolutePath)
            ? new RobotsCheck(null, AllowedMessage, requests, code)
            : new RobotsCheck(ScanStatus.SkippedRobots, "robots.txt disallows our crawler for the home page.", requests, code);
    }

    private static async Task<string> ReadLimitedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[MaxBytes];
        var total = 0;
        int read;
        while (total < MaxBytes && (read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken)) > 0)
        {
            total += read;
        }

        return System.Text.Encoding.UTF8.GetString(buffer, 0, total);
    }
}
