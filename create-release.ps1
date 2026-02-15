# PowerShell Script to Create GitHub Release Package
# This script builds, publishes, and packages the application for distribution

param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Creating Release Package v$Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$projectPath = ".\InventorySystem\InventorySystem.csproj"
$publishPath = ".\InventorySystem\bin\Release\net8.0-windows\win-x64\publish"
$releasePath = ".\releases"
$packageName = "InventorySystem-v$Version-win-x64"
$packagePath = "$releasePath\$packageName"

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $releasePath) {
    Remove-Item $releasePath -Recurse -Force
}
New-Item -ItemType Directory -Path $releasePath -Force | Out-Null

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Green
dotnet restore $projectPath

# Build in Release mode
Write-Host "Building in Release mode..." -ForegroundColor Green
dotnet build $projectPath --configuration $Configuration --no-restore

# Publish self-contained
Write-Host "Publishing self-contained package..." -ForegroundColor Green
dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $publishPath `
    /p:PublishSingleFile=true `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create package directory
Write-Host "Creating package directory..." -ForegroundColor Green
New-Item -ItemType Directory -Path $packagePath -Force | Out-Null

# Copy published files
Write-Host "Copying files to package..." -ForegroundColor Green
Copy-Item -Path "$publishPath\*" -Destination $packagePath -Recurse -Force

# Copy installation script
Copy-Item -Path ".\install.ps1" -Destination $packagePath -Force

# Create README for the package
Write-Host "Creating package README..." -ForegroundColor Green
$readmeContent = @"
# Inventory Management System v$Version

## Quick Start

### Option 1: Simple Installation (Recommended)
1. Right-click on ``install.ps1``
2. Select "Run with PowerShell"
3. Follow the on-screen instructions

The installer will:
- Copy files to your system
- Create desktop shortcut
- Create Start Menu entry

### Option 2: Manual Installation
1. Extract all files to a folder of your choice
2. Run ``InventorySystem.exe``

## System Requirements

- Windows 10 or Windows 11 (64-bit)
- No additional software required (includes .NET runtime)

## First Run

On first launch, the application will:
- Create a local SQLite database (``inventory.db``)
- Initialize the database schema

**Optional**: Click the "Generate Sample Data" button (magic wand icon) on the dashboard to populate with demo data.

## Features

- 📊 Real-time Analytics Dashboard
- 🛒 Complete Point of Sale System
- 📦 Inventory Management
- 👥 Customer Management
- 🚛 Supplier Management
- 📄 PDF Invoice Generation
- 📈 Sales History & Reporting

## Support

For issues or questions, please visit:
https://github.com/yourusername/InventorySystem/issues

## License

MIT License - See LICENSE file for details

---

Made with ❤️ using .NET 8 and WPF
"@

Set-Content -Path "$packagePath\README.txt" -Value $readmeContent

# Create ZIP package
Write-Host "Creating ZIP archive..." -ForegroundColor Green
$zipPath = "$releasePath\$packageName.zip"
Compress-Archive -Path $packagePath -DestinationPath $zipPath -Force

# Calculate file size
$zipSize = (Get-Item $zipPath).Length / 1MB
$zipSizeFormatted = "{0:N2} MB" -f $zipSize

# Generate release notes
Write-Host "Generating release notes..." -ForegroundColor Green
$releaseNotes = @"
# Release v$Version

## 📦 Installation

### Windows (64-bit)
Download ``$packageName.zip`` and extract it.

**Quick Install:**
1. Extract the ZIP file
2. Run ``install.ps1`` (right-click → Run with PowerShell)
3. Launch from desktop shortcut

**Manual Install:**
1. Extract the ZIP file
2. Run ``InventorySystem.exe``

## ✨ Features

- ✅ Real-time Analytics Dashboard with KPIs and charts
- ✅ Complete Point of Sale (POS) system
- ✅ Inventory Management with stock tracking
- ✅ Customer & Supplier Management
- ✅ Professional PDF Invoice Generation
- ✅ Sales History & Reporting
- ✅ Input Validation with visual feedback
- ✅ Active/Inactive status management
- ✅ Sample data generation for testing

## 🛠️ Technical Details

- **Framework**: .NET 8
- **Database**: SQLite (embedded)
- **Size**: $zipSizeFormatted
- **Platform**: Windows 10/11 (64-bit)
- **Dependencies**: None (self-contained)

## 📋 System Requirements

- Windows 10 or Windows 11 (64-bit)
- ~200 MB disk space
- No .NET runtime installation required

## 🐛 Known Issues

None at this time.
183: 
184: ## ⚠️ Troubleshooting / Antivirus Warnings
185: 
186: Because this is an open-source project without a costly digital code signing certificate, Windows **Microsoft Defender SmartScreen** or your antivirus might flag the installer as "Unknown Publisher".
187: 
188: **This is a false positive.** The code is open source and safe.
189: 
190: **If you see "Windows protected your PC":**
191: 1. Click **More info**
192: 2. Click **Run anyway**


## 📝 Changelog

### Added
- Initial release with core features
- Dashboard with real-time analytics
- Complete POS workflow
- PDF invoice generation
- Sample data seeding

---

**Full Changelog**: https://github.com/yourusername/InventorySystem/commits/v$Version
"@

Set-Content -Path "$releasePath\RELEASE_NOTES.md" -Value $releaseNotes

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " Release Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Package Details:" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor White
Write-Host "  Location: $zipPath" -ForegroundColor White
Write-Host "  Size: $zipSizeFormatted" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Test the package on a clean Windows machine" -ForegroundColor White
Write-Host "  2. Create a new GitHub Release" -ForegroundColor White
Write-Host "  3. Upload $packageName.zip" -ForegroundColor White
Write-Host "  4. Copy release notes from releases\RELEASE_NOTES.md" -ForegroundColor White
Write-Host ""
Write-Host "Release files are in: $releasePath" -ForegroundColor Cyan
Write-Host ""
