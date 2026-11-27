namespace NewDesk.Core;

/// <summary>Arguments passed from the shell to the watchdog.</summary>
public sealed record WatchdogArgs(int ShellPid, string ShellPath)
{
    public const string PidFlag = "--shell-pid";
    public const string PathFlag = "--shell-path";

    public string[] ToArgs() => [PidFlag, ShellPid.ToString(System.Globalization.CultureInfo.InvariantCulture), PathFlag, ShellPath];

    public static WatchdogArgs? TryParse(IReadOnlyList<string> args)
    {
        int? pid = null;
        string? path = null;
        for (var i = 0; i + 1 < args.Count; i++)
        {
            if (args[i] == PidFlag && int.TryParse(args[i + 1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var p) && p > 0)
                pid = p;
            else if (args[i] == PathFlag && !string.IsNullOrWhiteSpace(args[i + 1]))
                path = args[i + 1];
        }
        return pid is { } id && path is { } exe ? new WatchdogArgs(id, exe) : null;
    }
}
