using A11yReport.Scanner.Scanning;

namespace A11yReport.Tests.Integration;

/// <summary>One local server and one browser shared by all integration tests in a class.</summary>
public sealed class ScannerFixture : IAsyncLifetime
{
    public FixtureServer Server { get; } = new();

    public BrowserSession Session { get; private set; } = null!;

    public async Task InitializeAsync() => Session = await BrowserSession.StartAsync();

    public async Task DisposeAsync()
    {
        await Session.DisposeAsync();
        Server.Dispose();
    }
}
