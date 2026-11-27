# Manual recovery for NewDesk. Run it if the Windows taskbar is ever missing:
#   right-click this file > Run with PowerShell
# It closes NewDesk and restarts Explorer, which always brings the normal taskbar back.
# Needs no admin rights and changes no settings.

Get-Process NewDesk.Shell, NewDesk.Watchdog -ErrorAction SilentlyContinue | Stop-Process -Force
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
if (-not (Get-Process explorer -ErrorAction SilentlyContinue)) { Start-Process explorer.exe }
Write-Host "Windows taskbar restored."
