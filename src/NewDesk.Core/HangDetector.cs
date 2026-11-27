namespace NewDesk.Core;

/// <summary>
/// Turns periodic "is the shell responding?" samples into a single decision:
/// once the shell has been unresponsive for <see cref="Threshold"/>, the watchdog
/// gives Windows its taskbar back (it does not kill the shell).
/// </summary>
public sealed class HangDetector
{
    private DateTimeOffset? _unresponsiveSince;

    public HangDetector(TimeSpan? threshold = null) =>
        Threshold = threshold ?? TimeSpan.FromSeconds(15);

    public TimeSpan Threshold { get; }

    /// <summary>Returns true when the shell counts as hung at <paramref name="now"/>.</summary>
    public bool Sample(bool responding, DateTimeOffset now)
    {
        if (responding)
        {
            _unresponsiveSince = null;
            return false;
        }
        _unresponsiveSince ??= now;
        return now - _unresponsiveSince.Value >= Threshold;
    }
}
