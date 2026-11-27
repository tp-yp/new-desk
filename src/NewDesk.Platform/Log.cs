using System.Diagnostics;

namespace NewDesk.Platform;

/// <summary>Append-only log in %LOCALAPPDATA%\NewDesk\logs. Never throws.</summary>
public static class Log
{
    private static readonly object Gate = new();
    private const long MaxBytes = 1_000_000;

    public static string Directory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewDesk", "logs");

    public static string Source { get; set; } = Process.GetCurrentProcess().ProcessName;

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                System.IO.Directory.CreateDirectory(Directory);
                var file = Path.Combine(Directory, $"{Source}.log");
                if (File.Exists(file) && new FileInfo(file).Length > MaxBytes)
                    File.Move(file, file + ".old", overwrite: true);
                File.AppendAllText(file, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{Environment.ProcessId}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never be the thing that breaks the shell.
        }
    }
}
