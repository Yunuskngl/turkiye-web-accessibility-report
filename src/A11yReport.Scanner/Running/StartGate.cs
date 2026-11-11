namespace A11yReport.Scanner.Running;

/// <summary>
/// Global rate limit: lets one site start at a time, at least <c>interval</c> after the previous start,
/// no matter how many workers run in parallel.
/// </summary>
public sealed class StartGate(TimeSpan interval, TimeProvider time) : IDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTimeOffset? _lastStart;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var wait = _lastStart is { } last ? last + interval - time.GetUtcNow() : TimeSpan.Zero;
            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, time, cancellationToken);
            }

            _lastStart = time.GetUtcNow();
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose() => _lock.Dispose();
}
