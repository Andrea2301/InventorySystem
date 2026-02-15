#  How to Create and Publish a GitHub Release

This guide explains how to create a distributable package and publish it to GitHub Releases.

## Quick Start

### Step 1: Build the Release Package

Run the automated release script:

```powershell
.\create-release.ps1 -Version "1.0.0"
```

This will:
- ✅ Clean previous builds
- ✅ Restore NuGet dependencies  
- ✅ Build in Release mode
- ✅ Publish as self-contained (includes .NET runtime)
- ✅ Create a single executable
- ✅ Package everything into a ZIP file
- ✅ Generate release notes

**Output:** `releases/InventorySystem-v1.0.0-win-x64.zip` (~150-200 MB)

---

### Step 2: Test the Package (Important!)

Before publishing, test on a clean machine:

1. Extract the ZIP to a test folder
2. Run `install.ps1` or `InventorySystem.exe`
3. Verify all features work
4. Check that database is created correctly

---

### Step 3: Create GitHub Release

1. **Go to your GitHub repository**
   - Navigate to `https://github.com/yourusername/InventorySystem`

2. **Create a new release**
   - Click "Releases" → "Create a new release"
   - Tag version: `v1.0.0`
   - Release title: `v1.0.0 - Initial Release`

3. **Upload the ZIP file**
   - Drag and drop `InventorySystem-v1.0.0-win-x64.zip`

4. **Copy release notes**
   - Open `releases/RELEASE_NOTES.md`
   - Copy the content to the release description

5. **Publish**
   - Click "Publish release"

---

## What Users Will Download

When users download your release, they get:

```
InventorySystem-v1.0.0-win-x64.zip
├── InventorySystem.exe       # Main application (self-contained)
├── install.ps1                # Installation script
└── README.txt                 # Quick start guide
```

**No .NET installation required!** Everything is bundled.

---

## Installation Methods for Users

### Method 1: Automated Install (Recommended)
1. Extract ZIP
2. Right-click `install.ps1` → Run with PowerShell
3. Shortcuts created automatically

### Method 2: Manual
1. Extract ZIP
2. Run `InventorySystem.exe`

---

## Updating the Version

To create a new release (e.g., v1.1.0):

1. **Update version in `.csproj`:**
   ```xml
   <Version>1.1.0</Version>
   ```

2. **Run release script:**
   ```powershell
   .\create-release.ps1 -Version "1.1.0"
   ```

3. **Follow Step 3 above** to publish to GitHub

---

## Troubleshooting

### Build Fails
- Ensure .NET 8 SDK is installed
- Run `dotnet restore` first
- Check for compilation errors

### Package Too Large
- This is normal for self-contained apps (~150-200 MB)
- Includes entire .NET runtime
- Users don't need to install anything

### Installation Script Blocked
- Windows may block PowerShell scripts
- Users can right-click → Properties → Unblock
- Or run: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`

---

## Files Created

After running `create-release.ps1`:

```
releases/
├── InventorySystem-v1.0.0-win-x64/      # Extracted package
│   ├── InventorySystem.exe
│   ├── install.ps1
│   └── README.txt
├── InventorySystem-v1.0.0-win-x64.zip   # Upload this to GitHub
└── RELEASE_NOTES.md                      # Copy to release description
```

---

## Best Practices

✅ **Always test** the package before publishing  
✅ **Update version numbers** consistently  
✅ **Write clear release notes** with changelog  
✅ **Tag releases** properly in Git  
✅ **Keep old releases** available for users

---

## Next Steps

After publishing your first release:

1. Update README badges with actual release link
2. Add screenshots to the release
3. Share the release link in your portfolio
4. Consider creating a demo video

---

**Questions?** Check the main [README.md](../README.md) for more details.
