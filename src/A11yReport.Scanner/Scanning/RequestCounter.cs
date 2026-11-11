using A11yReport.Core.Results;
using Microsoft.Playwright;

namespace A11yReport.Scanner.Scanning;

/// <summary>Counts every request the browser makes for one page: main-frame document loads and everything else.</summary>
internal sealed class RequestCounter
{
    private int _documents;
    private int _subresources;

    public RequestCounts Counts => new(0, Volatile.Read(ref _documents), Volatile.Read(ref _subresources));

    public static RequestCounter Attach(IPage page)
    {
        var counter = new RequestCounter();
        page.Request += (_, request) => counter.Count(page, request);
        return counter;
    }

    private void Count(IPage page, IRequest request)
    {
        // Redirect hops of the main document count as document requests too.
        if (request.IsNavigationRequest && request.Frame == page.MainFrame)
        {
            Interlocked.Increment(ref _documents);
            return;
        }

        Interlocked.Increment(ref _subresources);
    }
}
