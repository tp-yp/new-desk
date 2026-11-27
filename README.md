# NewDesk

A replacement Windows 11 shell: taskbar, Start menu, system tray, quick panels, notification center,
clock and calendar, per-monitor desktops, with [Files](https://github.com/files-community/Files) as the file explorer.

**Rule one: nothing on the system may break.** NewDesk runs *alongside* `explorer.exe` and only hides
Explorer's taskbar. Every way out brings it back.

## Status: phase 0, the safety net

Phase 0 has no real UI yet. It hides Explorer's taskbar, shows a small panel in the corner,
and proves that every way out restores Windows.

| What happens | Result |
| --- | --- |
| "Restore Windows and exit" button, or closing the panel | Taskbar back, NewDesk and watchdog close |
| `Ctrl+Alt+Shift+F12` anywhere | Same as the button |
| Shell crashes or is killed | Watchdog restores the taskbar at once, then restarts the shell |
| 3 crashes within 2 minutes | Watchdog gives up; Windows stays on its normal taskbar |
| Shell stops responding for 15 s | Watchdog restores the taskbar (leaves the shell running) |
| Shift held while starting | Safe mode: nothing changes, NewDesk closes |
| Watchdog missing next to the shell | NewDesk refuses to hide anything |
| All else fails | Run `restore-taskbar.ps1`, or restart `explorer.exe` from Task Manager |

What NewDesk does **not** do: ask for admin rights, write to the registry, change system files,
inject into other processes, or change the screen work area.

## Testing on the spare PC

1. Download the `NewDesk-win-x64` artifact from the latest [build](../../actions/workflows/build.yml) run and unzip it.
2. Run `NewDesk.Shell.exe`. The taskbar disappears and a NewDesk panel appears bottom right.
3. Try each row in the table above. For the crash row use the panel's test button, or kill
   `NewDesk.Shell.exe` in Task Manager.
4. Logs: `%LOCALAPPDATA%\NewDesk\logs` (`shell.log`, `watchdog.log`).

## Layout

| Project | Role |
| --- | --- |
| `src/NewDesk.Core` | Platform-neutral logic (restart policy, hang detection), unit-tested |
| `src/NewDesk.Platform` | Win32 interop: Explorer taskbar, hotkey, safe mode, log |
| `src/NewDesk.Watchdog` | Small UI-less guardian process |
| `src/NewDesk.Shell` | The WPF shell (.NET 10, Fluent theme) |
| `tests/NewDesk.Core.Tests` | xUnit tests, run on any OS |

Build: `dotnet build NewDesk.sln` (.NET 10 SDK; builds on Linux too, runs only on Windows).

## Licenses

NewDesk will use [ManagedShell](https://github.com/cairoshell/ManagedShell) (Apache-2.0) from phase 1, and
integrates with Files (MIT) as a separate app.
