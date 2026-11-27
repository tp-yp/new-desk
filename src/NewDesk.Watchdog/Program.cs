using System.Diagnostics;
using NewDesk.Core;
using NewDesk.Platform;

namespace NewDesk.Watchdog;

/// <summary>
/// Watches the shell process. Whatever happens to the shell (clean exit, crash, being killed,
/// hanging), Explorer's taskbar comes back. After a crash it restarts the shell, but gives up
/// after 3 crashes in 2 minutes and leaves Windows on its normal taskbar.
/// </summary>
internal static class Program
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private static int Main(string[] args)
    {
        Log.Source = "watchdog";

        if (WatchdogArgs.TryParse(args) is not { } parsed)
        {
            Log.Write("Started without valid arguments; nothing to watch.");
            return 1;
        }

        using var mutex = new Mutex(initiallyOwned: true, Names.WatchdogMutex, out var createdNew);
        if (!createdNew)
        {
            Log.Write("Another watchdog is already running; exiting.");
            return 0;
        }

        try
        {
            Watch(parsed);
        }
        catch (Exception ex)
        {
            Log.Write($"Watchdog failed: {ex}");
        }
        finally
        {
            // Whatever went wrong above, never leave the user without a taskbar.
            ExplorerTaskbar.Restore();
        }
        return 0;
    }

    private static void Watch(WatchdogArgs args)
    {
        var policy = new RestartPolicy();
        var pid = args.ShellPid;
        Log.Write($"Watching shell pid {pid}.");

        while (true)
        {
            Process shell;
            try
            {
                shell = Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
                Log.Write($"Shell pid {pid} is gone before it could be watched.");
                return;
            }

            using (shell)
            {
                var exitCode = WaitForExit(shell);

                ExplorerTaskbar.Restore();

                if (exitCode is { } code && ShellExitCodes.IsDeliberate(code))
                {
                    Log.Write($"Shell exited on purpose (code {code}).");
                    return;
                }

                Log.Write($"Shell crashed (exit code {exitCode?.ToString() ?? "unknown"}). Taskbar restored.");
                if (!policy.RecordCrashAndCheck(DateTimeOffset.Now))
                {
                    Log.Write($"Shell crashed {policy.MaxCrashes} times within {policy.Window.TotalMinutes:0} minutes. Giving up; Windows stays on its normal taskbar.");
                    return;
                }

                if (!File.Exists(args.ShellPath))
                {
                    Log.Write($"Cannot restart: {args.ShellPath} not found.");
                    return;
                }

                using var restarted = Process.Start(new ProcessStartInfo(args.ShellPath, "--restarted") { UseShellExecute = false });
                if (restarted is null)
                {
                    Log.Write("Restart failed to start a process.");
                    return;
                }
                pid = restarted.Id;
                Log.Write($"Restarted shell as pid {pid}.");
            }
        }
    }

    /// <summary>Blocks until the shell exits. While waiting, gives the taskbar back if the shell hangs.</summary>
    private static int? WaitForExit(Process shell)
    {
        var hang = new HangDetector();
        var restoredForHang = false;

        while (!shell.WaitForExit(PollInterval))
        {
            bool responding;
            try
            {
                shell.Refresh();
                responding = shell.Responding;
            }
            catch (InvalidOperationException)
            {
                break; // exited between calls
            }

            if (hang.Sample(responding, DateTimeOffset.Now))
            {
                if (!restoredForHang)
                {
                    Log.Write($"Shell has not responded for {hang.Threshold.TotalSeconds:0}s. Restoring the taskbar; the shell is left running.");
                    ExplorerTaskbar.Restore();
                    restoredForHang = true;
                }
            }
            else if (responding)
            {
                restoredForHang = false;
            }
        }

        try
        {
            return shell.ExitCode;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
