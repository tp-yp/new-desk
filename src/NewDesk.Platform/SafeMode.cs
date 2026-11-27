namespace NewDesk.Platform;

public static class SafeMode
{
    /// <summary>True while either Shift key is held down.</summary>
    public static bool IsShiftHeld() => (Native.GetAsyncKeyState(Native.VK_SHIFT) & 0x8000) != 0;
}
