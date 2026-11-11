using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace A11yReport.Tests.Integration;

/// <summary>
/// Serves the HTML files in the Fixtures folder (or another folder) on 127.0.0.1, so tests never touch the internet.
/// "/" serves the home page file and "/robots.txt" serves the given text (404 when null).
/// Every requested path is recorded, so tests can prove which requests were made.
/// </summary>
public sealed class FixtureServer : IDisposable
{
    private const string FixtureFolder = "Fixtures";
    private const string RobotsPath = "/robots.txt";
    private const string SlowResourcePath = "/slow-resource";
    private static readonly TimeSpan SlowResourceDelay = TimeSpan.FromSeconds(20);
    private const string HtmlContentType = "text/html; charset=utf-8";
    private const string TextContentType = "text/plain; charset=utf-8";
    private const string CssContentType = "text/css; charset=utf-8";

    private readonly HttpListener _listener = new();
    private readonly string _root;
    private readonly string? _robotsTxt;
    private readonly string _homePage;

    public FixtureServer(string? robotsTxt = null, string homePage = "known-failures.html", string? rootFolder = null)
    {
        _robotsTxt = robotsTxt;
        _homePage = homePage;
        _root = rootFolder ?? Path.Combine(AppContext.BaseDirectory, FixtureFolder);
        BaseUrl = new Uri($"http://127.0.0.1:{FindFreePort()}/");
        _listener.Prefixes.Add(BaseUrl.ToString());
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    public Uri BaseUrl { get; }

    public ConcurrentQueue<string> RequestedPaths { get; } = new();

    public Uri UrlFor(string fileName) => new(BaseUrl, fileName);

    /// <summary>A local address where nothing listens, to test refused connections.</summary>
    public static Uri ClosedPortUrl() => new($"http://127.0.0.1:{FindFreePort()}/");

    public void Dispose() => _listener.Close();

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException)
            {
                return;
            }

            // Not awaited, so one slow response does not hold up the others.
            _ = RespondSafelyAsync(context);
        }
    }

    private async Task RespondSafelyAsync(HttpListenerContext context)
    {
        try
        {
            await RespondAsync(context);
        }
        catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException)
        {
            // The browser or the test went away before the response was written.
        }
    }

    private async Task RespondAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "/";
        RequestedPaths.Enqueue(path);
        if (path == SlowResourcePath)
        {
            await Task.Delay(SlowResourceDelay);
        }

        var (body, contentType) = Content(path);
        if (body is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
            return;
        }

        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = body.Length;
        await context.Response.OutputStream.WriteAsync(body);
        context.Response.Close();
    }

    private (byte[]? Body, string ContentType) Content(string path)
    {
        if (path == RobotsPath)
        {
            return (_robotsTxt is null ? null : Encoding.UTF8.GetBytes(_robotsTxt), TextContentType);
        }

        var fileName = path == "/" ? _homePage : Path.GetFileName(path);
        var file = Path.Combine(_root, fileName);
        var contentType = fileName.EndsWith(".css", StringComparison.Ordinal) ? CssContentType : HtmlContentType;
        return (fileName.Length > 0 && File.Exists(file) ? File.ReadAllBytes(file) : null, contentType);
    }

    private static int FindFreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }
}
