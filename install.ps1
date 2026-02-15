# PowerShell Installation Script for Inventory Management System
# This script helps users install the application with shortcuts

param(
    [string]$InstallPath = "$env:LOCALAPPDATA\InventorySystem"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Inventory Management System Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Note: Running without administrator privileges." -ForegroundColor Yellow
    Write-Host "Desktop shortcut creation may be limited to current user." -ForegroundColor Yellow
    Write-Host ""
}

# Create installation directory
Write-Host "Creating installation directory..." -ForegroundColor Green
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Copy files
Write-Host "Copying application files..." -ForegroundColor Green
$currentDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Copy-Item -Path "$currentDir\*" -Destination $InstallPath -Recurse -Force -Exclude "install.ps1"

# Create desktop shortcut
Write-Host "Creating desktop shortcut..." -ForegroundColor Green
$WshShell = New-Object -ComObject WScript.Shell
$DesktopPath = [System.Environment]::GetFolderPath('Desktop')
$ShortcutPath = "$DesktopPath\Inventory System.lnk"
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = "$InstallPath\InventorySystem.exe"
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.Description = "Inventory Management System"
$Shortcut.IconLocation = "$InstallPath\InventorySystem.exe,0"
$Shortcut.Save()

# Create Start Menu shortcut
Write-Host "Creating Start Menu shortcut..." -ForegroundColor Green
$StartMenuPath = [System.Environment]::GetFolderPath('StartMenu')
$StartMenuShortcut = "$StartMenuPath\Programs\Inventory System.lnk"
$StartShortcut = $WshShell.CreateShortcut($StartMenuShortcut)
$StartShortcut.TargetPath = "$InstallPath\InventorySystem.exe"
$StartShortcut.WorkingDirectory = $InstallPath
$StartShortcut.Description = "Inventory Management System"
$StartShortcut.IconLocation = "$InstallPath\InventorySystem.exe,0"
$StartShortcut.Save()

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " Installation Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Installation Location: $InstallPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now:" -ForegroundColor Yellow
Write-Host "  - Launch from Desktop shortcut" -ForegroundColor White
Write-Host "  - Launch from Start Menu" -ForegroundColor White
Write-Host "  - Run directly from: $InstallPath\InventorySystem.exe" -ForegroundColor White
Write-Host ""

# Ask if user wants to launch now
$launch = Read-Host "Would you like to launch the application now? (Y/N)"
if ($launch -eq 'Y' -or $launch -eq 'y') {
    Write-Host "Launching Inventory System..." -ForegroundColor Green
    Start-Process "$InstallPath\InventorySystem.exe"
}

Write-Host ""
Write-Host "Thank you for installing Inventory Management System!" -ForegroundColor Cyan
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
