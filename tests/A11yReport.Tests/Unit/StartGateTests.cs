using System.Diagnostics;
using A11yReport.Scanner.Running;

namespace A11yReport.Tests.Unit;

public sealed class StartGateTests
{
    [Fact]
    public async Task WaitAsync_SpacesStartsAcrossParallelCallers()
    {
        var interval = TimeSpan.FromMilliseconds(100);
        using var gate = new StartGate(interval, TimeProvider.System);
        var clock = Stopwatch.StartNew();

        await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => gate.WaitAsync(CancellationToken.None)));

        // Four starts need at least three full intervals between them.
        Assert.True(clock.Elapsed >= interval * 3 - TimeSpan.FromMilliseconds(15), $"took {clock.Elapsed}");
    }
}
