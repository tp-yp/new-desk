namespace NewDesk.Core;

/// <summary>
/// Decides whether the watchdog may restart the shell after a crash.
/// After <see cref="MaxCrashes"/> crashes inside <see cref="Window"/>, it gives up for good
/// and Windows stays on its normal taskbar until the user starts NewDesk again.
/// </summary>
public sealed class RestartPolicy
{
    private readonly Queue<DateTimeOffset> _crashes = new();

    public RestartPolicy(int maxCrashes = 3, TimeSpan? window = null)
    {
        if (maxCrashes < 1) throw new ArgumentOutOfRangeException(nameof(maxCrashes));
        MaxCrashes = maxCrashes;
        Window = window ?? TimeSpan.FromMinutes(2);
    }

    public int MaxCrashes { get; }
    public TimeSpan Window { get; }
    public bool GaveUp { get; private set; }

    /// <summary>Records a crash at <paramref name="now"/> and returns true if a restart is allowed.</summary>
    public bool RecordCrashAndCheck(DateTimeOffset now)
    {
        if (GaveUp) return false;

        _crashes.Enqueue(now);
        while (_crashes.Count > 0 && now - _crashes.Peek() > Window)
            _crashes.Dequeue();

        if (_crashes.Count >= MaxCrashes)
        {
            GaveUp = true;
            return false;
        }
        return true;
    }
}
