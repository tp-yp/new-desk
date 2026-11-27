namespace NewDesk.Platform;

/// <summary>
/// Hides and shows Explorer's own taskbars (main and secondary monitors).
/// Explorer keeps running untouched; only the visibility of its taskbar windows changes,
/// so restarting explorer.exe (e.g. from Task Manager) always brings the taskbar back.
/// The work area is deliberately not changed, so there is nothing else to undo.
/// </summary>
public static class ExplorerTaskbar
{
    private const string MainClass = "Shell_TrayWnd";
    private const string SecondaryClass = "Shell_SecondaryTrayWnd";

    public static IReadOnlyList<nint> FindAll()
    {
        var result = new List<nint>();
        var main = Native.FindWindowEx(0, 0, MainClass, null);
        if (main != 0) result.Add(main);

        nint next = 0;
        while ((next = Native.FindWindowEx(0, next, SecondaryClass, null)) != 0)
            result.Add(next);
        return result;
    }

    /// <summary>Hides every Explorer taskbar. Returns how many were hidden.</summary>
    public static int Hide()
    {
        var count = 0;
        foreach (var hwnd in FindAll())
        {
            if (Native.IsWindowVisible(hwnd))
            {
                Native.ShowWindow(hwnd, Native.SW_HIDE);
                count++;
            }
        }
        Log.Write($"Hid {count} Explorer taskbar window(s).");
        return count;
    }

    /// <summary>
    /// Shows every Explorer taskbar. Safe to call any number of times, from any process,
    /// even if NewDesk never hid anything.
    /// </summary>
    public static int Restore()
    {
        var count = 0;
        foreach (var hwnd in FindAll())
        {
            if (!Native.IsWindowVisible(hwnd))
            {
                // SW_SHOWNA: show without stealing focus from whatever the user is doing.
                Native.ShowWindow(hwnd, Native.SW_SHOWNA);
                count++;
            }
        }
        Log.Write($"Restored {count} Explorer taskbar window(s).");
        return count;
    }
}
