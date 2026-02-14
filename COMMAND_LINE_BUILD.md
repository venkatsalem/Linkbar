# Command Line Build Guide - Linkbar

## Overview

This guide shows you how to build Linkbar **entirely from command line** without using the Delphi IDE.

## Prerequisites

1. **Delphi installed** (see DOWNLOAD_DELPHI.md)
2. **Command Prompt** (cmd.exe) or PowerShell
3. **Linkbar source code** downloaded

## Quick Start

```batch
cd Linkbar
build.bat
```

That's it! The script handles everything.

---

## Detailed Command Line Build Process

### Step 1: Navigate to Linkbar Directory

```batch
cd D:\Venkat\Venkat-Browser\Linkbar
```

Replace with your actual path.

### Step 2: Build Using Script (Easiest)

```batch
build.bat
```

**What it does:**
1. Checks for Delphi installation
2. Finds dcc64.exe compiler
3. Builds Release configuration with optimizations
4. Creates executable in exe\LinkbarWin64.exe
5. Shows file size

### Step 3: Verify Build

```batch
dir exe\LinkbarWin64.exe
```

Expected: ~1,300,000 bytes (1.3 MB)

---

## Build Scripts Available

### 1. build.bat - Simple Build

```batch
cd Linkbar
build.bat
```

**Features:**
- Builds Release configuration
- All optimizations enabled
- No compression
- Simple and fast
- Output: ~1.3 MB

**When to use:**
- First build
- Quick testing
- Standard deployment

### 2. build_optimized.bat - Advanced Build

```batch
cd Linkbar
build_optimized.bat
```

**Features:**
- Cleans old build files
- Builds Release configuration
- Shows detailed size information
- Optional UPX compression
- Output: ~0.8-1.3 MB

**When to use:**
- Production builds
- Need smallest size
- Want compression options

### 3. english_only.bat - English-Only Localization

```batch
cd Linkbar
english_only.bat
```

**Features:**
- Removes non-English locale files
- Backs up original files first
- Saves ~200-300 KB
- Output: English-only installation

**When to use:**
- Only need English
- Minimal deployment size
- International not required

---

## Direct Delphi Compiler (Manual Build)

If you want to build directly without scripts:

### Find Delphi Path

```batch
dir "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe"
```

If found, use:
```
C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe
```

If not found, try:
```
C:\Program Files (x86)\Embarcadero\Studio\22.0\bin\dcc64.exe  (Delphi 10.4)
C:\Program Files (x86)\Embarcadero\Studio\21.0\bin\dcc64.exe  (Delphi 10.3)
```

### Direct Compiler Command

```batch
cd Linkbar\src
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" ^
  Linkbar.dpr ^
  -B ^
  -DRELEASE ^
  -$O+ ^
  -$L- ^
  -$C- ^
  -$Q+ ^
  -NSSystem;Vcl;Winapi ^
  -UEXE=..\exe\LinkbarWin64.exe
```

**Compiler Flags Explained:**

| Flag | Meaning |
|-------|---------|
| `-B` | Build all (compile changed units) |
| `-DRELEASE` | Define RELEASE symbol |
| `-$O+` | Enable optimization |
| `-$L-` | Disable local symbols |
| `-$C-` | Disable assertions |
| `-$Q+` | Enable stack frames elimination |
| `-NS` | Namespace list |
| `-UEXE` | Output directory |

### Simplified Command (All Defaults)

```batch
cd Linkbar\src
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" Linkbar.dpr
```

---

## MSBuild (Alternative Method)

If you have MSBuild (comes with Visual Studio):

### Build with MSBuild

```batch
cd Linkbar
msbuild src\Linkbar.dproj /p:Configuration=Release /p:Platform=Win64 /t:Build /v:minimal
```

**MSBuild Options:**

| Option | Meaning |
|--------|---------|
| `/p:Configuration=Release` | Use Release configuration |
| `/p:Platform=Win64` | Target 64-bit Windows |
| `/t:Build` | Build target |
| `/v:minimal` | Minimal verbosity (less output) |

### Clean and Rebuild

```batch
msbuild src\Linkbar.dproj /p:Configuration=Release /p:Platform=Win64 /t:Clean,Build /v:minimal
```

---

## Advanced Command Line Options

### Custom Output Directory

```batch
cd Linkbar\src
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" ^
  Linkbar.dpr ^
  -UEXE=C:\MyBuilds\Linkbar.exe
```

### Specific Delphi Version

```batch
set DELPHI_VER=23.0
"C:\Program Files (x86)\Embarcadero\Studio\%DELPHI_VER%\bin\dcc64.exe" ^
  Linkbar.dpr
```

### With Debug Information (for Troubleshooting)

```batch
cd Linkbar\src
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" ^
  Linkbar.dpr ^
  -DDEBUG ^
  -$D+ ^
  -$L+ ^
  -$C+ ^
  -V
```

**Debug Flags:**

| Flag | Meaning |
|-------|---------|
| `-DDEBUG` | Define DEBUG symbol |
| `-$D+` | Enable debug information |
| `-$L+` | Enable local symbols |
| `-$C+` | Enable assertions |
| `-V` | Generate map file |

---

## Environment Variables (Optional)

### Set Delphi Path Permanently

```batch
setx DELPHI_PATH "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin" /M
```

Then use simply:
```batch
%DELPHI_PATH%\dcc64.exe Linkbar.dpr
```

### Set Output Directory

```batch
set OUTPUT_DIR=C:\MyBuilds
```

Then use in build:
```batch
dcc64.exe Linkbar.dpr -UEXE=%OUTPUT_DIR%\Linkbar.exe
```

---

## Build Scenarios

### Scenario 1: First Build

```batch
cd Linkbar
build.bat
```

### Scenario 2: Clean Build

```batch
cd Linkbar
if exist exe\LinkbarWin64.exe del exe\LinkbarWin64.exe
if exist src\*.dcu del src\*.dcu
build.bat
```

### Scenario 3: Production Build (Smallest Size)

```batch
cd Linkbar
build_optimized.bat
rem Answer "y" when asked about UPX compression
```

### Scenario 4: English-Only Build

```batch
cd Linkbar
build.bat
english_only.bat
```

### Scenario 5: Debug Build (for Troubleshooting)

```batch
cd Linkbar\src
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" ^
  Linkbar.dpr ^
  -DDEBUG ^
  -$D+ ^
  -V
```

---

## Verifying Build Success

### Check File Exists

```batch
dir exe\LinkbarWin64.exe
```

Should show file with size ~1.3 MB.

### Check File Size

```batch
cd exe
for %A in (LinkbarWin64.exe) do @echo Size: %~zA bytes
```

Should be: 1,200,000 - 1,500,000 bytes

### Test Execution

```batch
cd exe
LinkbarWin64.exe
```

Should open Linkbar window in ~200ms.

### Check Dependencies

```batch
where LinkbarWin64.exe
dumpbin /dependents exe\LinkbarWin64.exe
```

Should show only Windows DLLs (no extra dependencies).

---

## Troubleshooting Command Line Builds

### Problem: "dcc64.exe not found"

**Check Delphi installation:**
```batch
dir "C:\Program Files (x86)\Embarcadero\Studio\*\bin\dcc64.exe" /s
```

**Solution:** Install Delphi or update the path in build.bat

### Problem: "File not found: Linkbar.dpr"

**Check current directory:**
```batch
cd
dir Linkbar.dpr
```

**Solution:**
```batch
cd Linkbar\src
```

### Problem: Build fails with errors

**Get detailed error output:**
```batch
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" ^
  Linkbar.dpr ^
  -V
```

The `-V` flag generates a map file with details.

### Problem: Output file too large (>2 MB)

**Check configuration:**
```batch
cd Linkbar
type src\linkbar.inc
```

Ensure `{$IFDEF RELEASE}` section has optimizations.

**Solution:**
```batch
cd Linkbar
del src\*.dcu
build.bat
```

### Problem: "Access denied"

**Run as Administrator:**
```batch
# Right-click Command Prompt
# Select "Run as administrator"
```

Or use PowerShell:
```powershell
Start-Process cmd -Verb RunAs
```

---

## Command Line Tips

### Use PowerShell for Better Scripting

```powershell
# Navigate to directory
cd D:\Venkat\Venkat-Browser\Linkbar

# Build
.\build.bat

# Check file size
(Get-Item .\exe\LinkbarWin64.exe).Length

# Format size
"{0:N2} MB" -f ((Get-Item .\exe\LinkbarWin64.exe).Length / 1MB)
```

### Create Batch File for Frequent Builds

Create `build_custom.bat`:
```batch
@echo off
echo Building Linkbar...
cd /d %~dp0
call build.bat
if exist exe\LinkbarWin64.exe (
    echo.
    echo Build successful!
    echo Testing...
    exe\LinkbarWin64.exe
)
```

Then just run:
```batch
build_custom.bat
```

### Use Relative Paths

```batch
# Instead of:
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" Linkbar.dpr

# Use build script:
build.bat
```

---

## Build Output Files

### After Successful Build

```
Linkbar\
├── exe\
│   └── LinkbarWin64.exe        # Main executable (~1.3 MB)
├── src\
│   ├── *.dcu                    # Compiled units (intermediate)
│   └── *.map                    # Debug map (if -V flag used)
└── out\
    └── linkbar64\               # DCU output directory
        └── *.dcu                # Additional compiled units
```

### Files You Can Delete

After successful build, you can delete:
```batch
cd Linkbar
if exist src\*.dcu del src\*.dcu
if exist src\*.map del src\*.map
if exist out rmdir /s /q out
```

---

## Batch Script Customization

### Modify build.bat for Your System

Edit `build.bat` and update paths:

```batch
REM Change this line:
set DELPHI_PATH=C:\Program Files (x86)\Embarcadero\Studio\23.0\bin

REM To your actual path:
set DELPHI_PATH=C:\MyCustomPath\Delphi\bin
```

### Add Custom Compiler Flags

Edit `build.bat` and modify dcc64 command:

```batch
"%DELPHI_PATH%\dcc64.exe" src\Linkbar.dpr ^
  -B ^
  -DRELEASE ^
  -$O+ ^
  -$L- ^
  -YOUR_CUSTOM_FLAG ^
  -NSSystem;Vcl;Winapi
```

---

## Summary: Command Line Build Workflow

**Complete workflow from scratch:**

```batch
# 1. Download and install Delphi (see DOWNLOAD_DELPHI.md)

# 2. Navigate to Linkbar directory
cd D:\Venkat\Venkat-Browser\Linkbar

# 3. Build
build.bat

# 4. Verify
dir exe\LinkbarWin64.exe

# 5. Test
exe\LinkbarWin64.exe

# 6. Optional: English-only
english_only.bat

# 7. Optional: Compress with UPX
upx --best --lzma exe\LinkbarWin64.exe
```

**Time required:**
- Build: ~10-30 seconds
- English-only: ~1 second
- UPX compression: ~5-10 seconds
- **Total**: ~1-2 minutes

---

## Need More Help?

- **Download Delphi**: See DOWNLOAD_DELPHI.md
- **Build issues**: See BUILD_WIN11.md
- **Optimization**: See OPTIMIZATION.md
- **All changes**: See CHANGES.md

---

**Ready to build!**
```batch
cd Linkbar
build.bat
```
