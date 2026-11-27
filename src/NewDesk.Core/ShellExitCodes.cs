namespace NewDesk.Core;

/// <summary>Exit codes the shell uses to tell the watchdog why it stopped.</summary>
public static class ShellExitCodes
{
    /// <summary>The user closed NewDesk on purpose. The watchdog restores Windows and exits.</summary>
    public const int CleanExit = 0;

    /// <summary>Started while Shift was held: safe mode, nothing was changed.</summary>
    public const int SafeMode = 2;

    /// <summary>Another copy is already running.</summary>
    public const int AlreadyRunning = 3;

    /// <summary>Any exit code that is not a deliberate stop counts as a crash.</summary>
    public static bool IsDeliberate(int code) =>
        code is CleanExit or SafeMode or AlreadyRunning;
}
