using NewDesk.Core;
using Xunit;

namespace NewDesk.Core.Tests;

public class RestartPolicyTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Allows_restarts_until_the_third_crash_in_the_window()
    {
        var policy = new RestartPolicy(maxCrashes: 3, window: TimeSpan.FromMinutes(2));
        Assert.True(policy.RecordCrashAndCheck(T0));
        Assert.True(policy.RecordCrashAndCheck(T0.AddSeconds(30)));
        Assert.False(policy.RecordCrashAndCheck(T0.AddSeconds(60)));
        Assert.True(policy.GaveUp);
    }

    [Fact]
    public void Old_crashes_fall_out_of_the_window()
    {
        var policy = new RestartPolicy(maxCrashes: 3, window: TimeSpan.FromMinutes(2));
        Assert.True(policy.RecordCrashAndCheck(T0));
        Assert.True(policy.RecordCrashAndCheck(T0.AddMinutes(1)));
        Assert.True(policy.RecordCrashAndCheck(T0.AddMinutes(3)));
        Assert.True(policy.RecordCrashAndCheck(T0.AddMinutes(6)));
        Assert.False(policy.GaveUp);
    }

    [Fact]
    public void Once_given_up_it_never_restarts_again()
    {
        var policy = new RestartPolicy(maxCrashes: 1);
        Assert.False(policy.RecordCrashAndCheck(T0));
        Assert.False(policy.RecordCrashAndCheck(T0.AddHours(1)));
    }
}

public class HangDetectorTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Reports_hang_only_after_the_threshold()
    {
        var hang = new HangDetector(TimeSpan.FromSeconds(15));
        Assert.False(hang.Sample(false, T0));
        Assert.False(hang.Sample(false, T0.AddSeconds(14)));
        Assert.True(hang.Sample(false, T0.AddSeconds(15)));
    }

    [Fact]
    public void A_responsive_sample_resets_the_clock()
    {
        var hang = new HangDetector(TimeSpan.FromSeconds(15));
        Assert.False(hang.Sample(false, T0));
        Assert.False(hang.Sample(true, T0.AddSeconds(10)));
        Assert.False(hang.Sample(false, T0.AddSeconds(20)));
        Assert.True(hang.Sample(false, T0.AddSeconds(35)));
    }
}

public class ExitCodeAndArgsTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(1, false)]
    [InlineData(-1, false)]
    [InlineData(unchecked((int)0xE0434352), false)] // .NET unhandled exception
    public void Only_known_codes_are_deliberate(int code, bool deliberate) =>
        Assert.Equal(deliberate, ShellExitCodes.IsDeliberate(code));

    [Fact]
    public void Watchdog_args_round_trip()
    {
        var args = new WatchdogArgs(1234, @"C:\Program Files\NewDesk\NewDesk.Shell.exe");
        Assert.Equal(args, WatchdogArgs.TryParse(args.ToArgs()));
    }

    public static TheoryData<string[]> BadArgs => new()
    {
        Array.Empty<string>(),
        new[] { "--shell-pid", "abc", "--shell-path", "x.exe" },
        new[] { "--shell-pid", "-5", "--shell-path", "x.exe" },
        new[] { "--shell-pid", "12" },
    };

    [Theory]
    [MemberData(nameof(BadArgs))]
    public void Bad_watchdog_args_are_rejected(string[] raw) =>
        Assert.Null(WatchdogArgs.TryParse(raw));
}
