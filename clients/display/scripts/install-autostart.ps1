# Luminous Display - Auto-start Installation Script (Windows)
#
# This script configures the display app to start automatically on boot.
# Creates a scheduled task that runs at user login.

param(
    [string]$InstallPath = "C:\Program Files\Luminous Display"
)

$AppName = "Luminous Display"
$TaskName = "LuminousDisplay"

Write-Host "Luminous Display - Auto-start Configuration"
Write-Host "============================================"
Write-Host ""

# Check for admin rights
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Warning: Running without admin rights. Creating user-level autostart." -ForegroundColor Yellow
    Write-Host ""
}

$ExePath = Join-Path $InstallPath "Luminous Display.exe"

if (-not (Test-Path $ExePath)) {
    Write-Host "Warning: Executable not found at $ExePath" -ForegroundColor Yellow
    Write-Host "Please ensure the app is installed correctly." -ForegroundColor Yellow
}

# Method 1: Scheduled Task (preferred for kiosk mode)
if ($isAdmin) {
    Write-Host "Creating scheduled task for auto-start..."

    # Remove existing task if present
    $existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($existingTask) {
        Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
        Write-Host "Removed existing task."
    }

    # Create new task
    $action = New-ScheduledTaskAction -Execute $ExePath
    $trigger = New-ScheduledTaskTrigger -AtLogOn
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description "Starts Luminous Display at system startup"

    Write-Host "Scheduled task created successfully." -ForegroundColor Green
}

# Method 2: Startup folder shortcut (fallback for non-admin)
$StartupPath = [Environment]::GetFolderPath('Startup')
$ShortcutPath = Join-Path $StartupPath "$AppName.lnk"

Write-Host "Creating startup shortcut..."

$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = $ExePath
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.Description = "Luminous Display - Family Command Center"
$Shortcut.Save()

Write-Host "Startup shortcut created at: $ShortcutPath" -ForegroundColor Green

Write-Host ""
Write-Host "Auto-start configuration complete!"
Write-Host "The display app will start automatically on next login."
