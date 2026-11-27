using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using NewDesk.Core;
using NewDesk.Platform;

namespace NewDesk.Shell;

/// <summary>
/// Phase 0: proves the safety net. The shell hides Explorer's taskbar only after the watchdog
/// is running, and every way out (button, hotkey, crash, kill, hang) brings the taskbar back.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstance;
    private EmergencyHotkey? _hotkey;
    private bool _restored;

    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Source = "shell";
        Log.Write($"Starting. Args: {string.Join(' ', e.Args)}");

        if (SafeMode.IsShiftHeld())
        {
            Log.Write("Shift held: safe mode, nothing changed.");
            MessageBox.Show(Strings.SafeModeMessage, Strings.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown(ShellExitCodes.SafeMode);
            return;
        }

        _singleInstance = new Mutex(initiallyOwned: true, Names.ShellMutex, out var createdNew);
        if (!createdNew)
        {
            Log.Write("Already running; exiting.");
            Shutdown(ShellExitCodes.AlreadyRunning);
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, args) => RestoreWindows($"unhandled exception: {args.ExceptionObject}");
        DispatcherUnhandledException += (_, args) => RestoreWindows($"UI thread exception: {args.Exception}");
        AppDomain.CurrentDomain.ProcessExit += (_, _) => RestoreWindows("process exit");

        if (!EnsureWatchdog())
        {
            MessageBox.Show(Strings.NoWatchdogMessage, Strings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown(ShellExitCodes.CleanExit);
            return;
        }

        _hotkey = new EmergencyHotkey();
        ComponentDispatcher.ThreadFilterMessage += OnThreadMessage;

        ExplorerTaskbar.Hide();

        var window = new ControlWindow(_hotkey.Registered);
        window.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ComponentDispatcher.ThreadFilterMessage -= OnThreadMessage;
        _hotkey?.Dispose();
        RestoreWindows($"exit code {e.ApplicationExitCode}");
        base.OnExit(e);
    }

    /// <summary>The user's way out: give Windows back and close cleanly (the watchdog then exits too).</summary>
    public bool IsExiting { get; private set; }

    public void RestoreAndExit()
    {
        if (IsExiting) return;
        IsExiting = true;
        RestoreWindows("user asked to exit");
        Shutdown(ShellExitCodes.CleanExit);
    }

    /// <summary>For testing the watchdog: die instantly, with no cleanup code running at all.</summary>
    public static void SimulateHardCrash()
    {
        Log.Write("Simulating a hard crash (process kill).");
        Process.GetCurrentProcess().Kill();
    }

    private void OnThreadMessage(ref MSG msg, ref bool handled)
    {
        if (EmergencyHotkey.IsHotkeyMessage(msg.message, msg.wParam))
        {
            Log.Write($"Emergency hotkey {EmergencyHotkey.Description} pressed.");
            handled = true;
            RestoreAndExit();
        }
    }

    private void RestoreWindows(string reason)
    {
        if (_restored) return;
        _restored = true;
        Log.Write($"Restoring Windows ({reason}).");
        ExplorerTaskbar.Restore();
    }

    /// <summary>Starts the watchdog unless one is already running (it is when the watchdog restarted us).</summary>
    private static bool EnsureWatchdog()
    {
        if (Mutex.TryOpenExisting(Names.WatchdogMutex, out var existing))
        {
            existing.Dispose();
            Log.Write("Watchdog already running.");
            return true;
        }

        var watchdogPath = Path.Combine(AppContext.BaseDirectory, Names.WatchdogExe);
        var shellPath = Environment.ProcessPath;
        if (!File.Exists(watchdogPath) || shellPath is null)
        {
            Log.Write($"Watchdog not found at {watchdogPath}; refusing to hide the taskbar.");
            return false;
        }

        var info = new ProcessStartInfo(watchdogPath) { UseShellExecute = false };
        foreach (var arg in new WatchdogArgs(Environment.ProcessId, shellPath).ToArgs())
            info.ArgumentList.Add(arg);

        using var watchdog = Process.Start(info);
        if (watchdog is null)
        {
            Log.Write("Watchdog failed to start; refusing to hide the taskbar.");
            return false;
        }

        // Give it a moment to take its mutex, so a quick crash right after this is still covered.
        for (var i = 0; i < 20; i++)
        {
            if (Mutex.TryOpenExisting(Names.WatchdogMutex, out var m)) { m.Dispose(); break; }
            if (watchdog.HasExited)
            {
                Log.Write($"Watchdog exited immediately (code {watchdog.ExitCode}); refusing to hide the taskbar.");
                return false;
            }
            Thread.Sleep(50);
        }

        Log.Write($"Watchdog started as pid {watchdog.Id}.");
        return true;
    }
}
