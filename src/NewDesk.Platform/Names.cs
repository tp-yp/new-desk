namespace NewDesk.Platform;

/// <summary>Per-session object names, so two users on one PC never collide.</summary>
public static class Names
{
    public const string ShellMutex = @"Local\NewDesk.Shell";
    public const string WatchdogMutex = @"Local\NewDesk.Watchdog";
    public const string WatchdogExe = "NewDesk.Watchdog.exe";
}
