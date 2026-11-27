namespace NewDesk.Platform;

/// <summary>
/// Ctrl+Alt+Shift+F12, registered for the whole system. Pressing it restores Windows
/// and closes NewDesk, even when no NewDesk window can be seen or clicked.
/// The hotkey is posted to the registering thread's message queue (hWnd = 0).
/// </summary>
public sealed class EmergencyHotkey : IDisposable
{
    public const int WM_HOTKEY = 0x0312;
    public const int Id = 0x4E44; // "ND"
    private const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_NOREPEAT = 0x4000;
    private const uint VK_F12 = 0x7B;

    public const string Description = "Ctrl+Alt+Shift+F12";

    public bool Registered { get; }

    public EmergencyHotkey()
    {
        Registered = Native.RegisterHotKey(0, Id, MOD_CONTROL | MOD_ALT | MOD_SHIFT | MOD_NOREPEAT, VK_F12);
        Log.Write(Registered
            ? $"Emergency hotkey {Description} registered."
            : $"Could not register {Description} (another app may own it). The on-screen button still works.");
    }

    public static bool IsHotkeyMessage(int msg, nint wParam) => msg == WM_HOTKEY && wParam == Id;

    public void Dispose()
    {
        if (Registered) Native.UnregisterHotKey(0, Id);
    }
}
