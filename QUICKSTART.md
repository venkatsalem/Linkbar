# Quick Start Guide - Download Delphi & Build Linkbar

## Fastest Path to Success

### 1. Download Delphi (Free)

**Option A: Web Installer (Recommended - Faster)**
1. Go to: https://www.embarcadero.com/products/delphi
2. Click "Download"
3. Create free account
4. Download "Web Installer" (~50 MB)
5. Run installer (15-30 minutes)

**Option B: Full Download (Slower, but offline)**
1. Same registration as above
2. Download "Full ISO" (~6-8 GB)
3. Run installer (15-30 minutes)

### 2. Install Delphi

During installation:
- ✅ Select "Custom" installation
- ✅ Choose "Delphi" and "Windows 64-bit"
- ✅ Select "VCL Framework"
- ❌ Uncheck: Database, Web, Mobile, Linux (not needed)
- Installation time: 15-30 minutes

### 3. Build Linkbar (Command Line)

```batch
cd Linkbar
build.bat
```

That's it! Executable will be in: `exe\LinkbarWin64.exe`

---

## One-Page Summary

### Download Delphi
```
URL: https://www.embarcadero.com/products/delphi
Account: Free registration required
Size: 50 MB (installer) or 6-8 GB (full)
Time: 1-2 hours (download + install)
```

### Build Commands

```batch
# Simple build
cd Linkbar
build.bat

# Optimized build (smallest size)
build_optimized.bat

# English-only localization
english_only.bat

# Direct compiler (manual)
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" src\Linkbar.dpr
```

### Delphi Paths

| Version | Path |
|---------|------|
| Delphi 11.0 (Athens) | `C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe` |
| Delphi 10.4 (Sydney) | `C:\Program Files (x86)\Embarcadero\Studio\22.0\bin\dcc64.exe` |
| Delphi 10.3 (Rio) | `C:\Program Files (x86)\Embarcadero\Studio\21.0\bin\dcc64.exe` |

### Build Output

```
Linkbar\
└── exe\
    └── LinkbarWin64.exe    # ~1.3 MB (optimized)
```

---

## Common Commands

### Check Delphi Installation
```batch
dir "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe"
```

### Verify Build
```batch
dir exe\LinkbarWin64.exe
```

### Test Build
```batch
cd exe
LinkbarWin64.exe
```

### Clean Build
```batch
cd Linkbar
if exist exe\LinkbarWin64.exe del exe\LinkbarWin64.exe
if exist src\*.dcu del src\*.dcu
build.bat
```

---

## Troubleshooting Quick Fixes

| Problem | Solution |
|---------|----------|
| "dcc64.exe not found" | Install Delphi or update path in build.bat |
| Build fails | Check Delphi version (must be 10.3+) |
| File too large (>2 MB) | Ensure Release configuration selected |
| "Access denied" | Run Command Prompt as Administrator |

---

## Documentation Files

| File | Description |
|------|-------------|
| **README.md** | Main project readme |
| **QUICKSTART.md** | This file |
| **DOWNLOAD_DELPHI.md** | Detailed Delphi download and installation guide |
| **COMMAND_LINE_BUILD.md** | Comprehensive command line build instructions |
| **BUILD_WIN11.md** | Windows 11 64-bit build instructions |
| **SECURITY.md** | Security policy and verification |
| **OPTIMIZATION_SUMMARY.md** | Optimization summary and benchmarks |
| **OPTIMIZATION.md** | Detailed optimization guide |
| **CHANGES.md** | Complete changes summary |

---

## Complete Workflow (30 Minutes)

```batch
# 1. Download Delphi (20-30 minutes)
#    Go to: https://www.embarcadero.com/products/delphi
#    Register (5 minutes)
#    Download installer (15-20 minutes)
#    Install (5-10 minutes)

# 2. Build Linkbar (30 seconds)
cd Linkbar
build.bat

# 3. Test (5 seconds)
cd exe
LinkbarWin64.exe

# 4. Done!
```

**Total time:** ~1 hour (mostly download time)
**Actual work time:** ~2 minutes

---

## Key Points

✅ Delphi Community Edition is **FREE**
✅ No license cost
✅ No time limit
✅ Valid forever
✅ Build entirely from command line
✅ No IDE required

---

## Need Detailed Help?

- **Download Delphi**: See DOWNLOAD_DELPHI.md
- **Build from command line**: See COMMAND_LINE_BUILD.md
- **Build issues**: See BUILD_WIN11.md
- **Optimization**: See OPTIMIZATION.md

---

**Get Started Now:**
1. Download Delphi: https://www.embarcadero.com/products/delphi
2. Install and run: `build.bat`
3. Done!
