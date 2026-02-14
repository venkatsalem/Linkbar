# Download and Install Delphi - Complete Guide

## Overview

This guide shows you how to download and install **Embarcadero Delphi Community Edition** (free) and build Linkbar entirely from command line.

## Step 1: Download Delphi Community Edition (Free)

### Official Download Page
Go to: https://www.embarcadero.com/products/delphi

### Alternative Direct Link
Community Edition registration page:
https://www.embarcadero.com/products/delphi/starter

## Step 2: Register for Free Account

Before downloading, you need a free Embarcadero account:

1. Go to: https://www.embarcadero.com/products/delphi
2. Click "Download" or "Get It Now"
3. Create account if you don't have one
4. Fill in basic information (name, email, etc.)
5. Accept license agreement
6. Registration is **free** for individual developers

## Step 3: Download Delphi

After registration, you'll get access to download:

### Download Options:
1. **Web Installer** (Recommended)
   - Smaller initial download (~50 MB)
   - Downloads components on demand
   - More flexible installation
   - URL: Provided after registration

2. **Full ISO** (~6-8 GB)
   - Complete package
   - Can install offline
   - URL: Provided after registration

3. **Direct Download Links** (if available):
   - Typically provided after registration
   - Check your email for download links

### Typical Download Steps:
1. Log into Embarcadero account
2. Go to "My Registered User Downloads"
3. Find "Delphi Community Edition"
4. Choose version (11.0 Athens recommended, or 10.4 Sydney)
5. Download installer

## Step 4: Install Delphi

### System Requirements

**Minimum:**
- Windows 10/11 64-bit
- 16 GB RAM recommended
- 20 GB free disk space
- 1920x1080 display

**Recommended:**
- Windows 11 64-bit
- 32 GB RAM
- SSD for faster builds
- 1920x1080 or higher

### Installation Steps

1. **Run Installer**
   ```
   Double-click: delphic_xxxx_xxxx_win64.exe
   ```

2. **Select Installation Type**
   - Choose "Custom" installation
   - This allows you to select only what you need

3. **Select Required Components**

   For building Linkbar, you need:
   ```
   ✓ Delphi (Required)
   ✓ Windows 64-bit Platform (Required)
   ✓ VCL Framework (Required)
   ✓ Samples and Help (Optional)
   ```

   **You can UNCHECK:**
   ```
   ✓ Database components (not needed)
   ✓ FireDAC (not needed)
   ✓ Web components (not needed)
   ✓ Mobile platforms (not needed)
   ✓ Linux/MacOS (not needed)
   ```

4. **Choose Installation Location**
   ```
   Default: C:\Program Files (x86)\Embarcadero\Studio\23.0\
   Or: Choose different drive if needed
   ```

5. **Start Installation**
   - Click "Install"
   - Wait for installation to complete (15-30 minutes)
   - Depends on internet speed and selected components

6. **Finish**
   - Click "Finish"
   - You may need to restart

## Step 5: Verify Installation

After installation, verify Delphi is installed:

### Check Files Exist
```batch
dir "C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe"
```

Should show dcc64.exe file.

### Check Version
```batch
"C:\Program Files (x86)\Embarcadero\Studio\23.0\bin\dcc64.exe" -version
```

Should show version information.

## Step 6: Add to PATH (Optional)

To use Delphi compiler from anywhere, add to system PATH:

### Method 1: Command Line (Requires Admin)
```batch
setx PATH "%PATH%;C:\Program Files (x86)\Embarcadero\Studio\23.0\bin" /M
```

### Method 2: Manual
1. Press `Win + R`, type: `sysdm.cpl`
2. Go to "Advanced" tab
3. Click "Environment Variables"
4. Under "System variables", find "Path"
5. Click "Edit"
6. Add: `C:\Program Files (x86)\Embarcadero\Studio\23.0\bin`
7. Click OK on all dialogs
8. Restart Command Prompt

## Alternative: Older Delphi Versions

If you prefer older versions (smaller download):

### Delphi 10.4 Sydney (Community Edition)
- More stable than latest
- Good for Windows development
- Download: https://www.embarcadero.com/products/delphi (via account)

### Delphi 10.3 Rio (Community Edition)
- Minimum version for Linkbar
- Very stable
- Smaller download size
- Download: https://www.embarcadero.com/products/delphi (via account)

## Installation Paths by Version

| Delphi Version | Installation Path |
|---------------|------------------|
| Delphi 11.0 (Athens) | `C:\Program Files (x86)\Embarcadero\Studio\23.0\` |
| Delphi 10.4 (Sydney) | `C:\Program Files (x86)\Embarcadero\Studio\22.0\` |
| Delphi 10.3 (Rio) | `C:\Program Files (x86)\Embarcadero\Studio\21.0\` |

## Troubleshooting Installation

### Problem: Download fails or is slow
**Solution**:
- Use web installer instead of full ISO
- Check internet connection
- Try downloading during off-peak hours
- Use download manager for large files

### Problem: Installation fails
**Solution**:
- Run installer as Administrator
- Disable antivirus temporarily
- Check available disk space (need 20+ GB)
- Install to different drive if C: is full
- Close other applications during installation

### Problem: Can't find dcc64.exe
**Solution**:
- Verify installation completed successfully
- Check installation path (see table above)
- Ensure you selected "Windows 64-bit" during installation
- Search for dcc64.exe on your system

### Problem: Large download size (6-8 GB)
**Solution**:
- Use web installer instead of full ISO
- Uncheck unnecessary components during installation
- Choose "Custom" installation and select minimum requirements

## After Installation

Once Delphi is installed:

1. **Test compiler**:
   ```batch
   cd Linkbar
   build.bat
   ```

2. **If build succeeds**, you're ready!

3. **If build fails**, see BUILD_WIN11.md for troubleshooting

## Licensing and Activation

### Community Edition
- **Free** for individual developers
- **No cost** to download and use
- **No revenue limit** (unlimited use)
- **Valid forever** (no expiration)

### Limitations
- For individual developers only
- Not for commercial use by companies
- Requires registration (email)

### Activation
After installation, you may need to:
1. Open Delphi IDE
2. Enter your registered email
3. License will activate automatically

## Support

### Official Support
- Delphi Documentation: https://docwiki.embarcadero.com/RADStudio/Alexandria/en/
- Community Forums: https://forums.embarcadero.com/
- Support: https://support.embarcadero.com/

### For Linkbar
- Build Issues: See BUILD_WIN11.md
- Optimization: See OPTIMIZATION.md
- Security: See SECURITY.md

## Summary

**Quick Steps:**
1. Register at https://www.embarcadero.com/products/delphi
2. Download Delphi Community Edition (free)
3. Run installer (15-30 minutes)
4. Verify dcc64.exe exists
5. Build Linkbar: `cd Linkbar && build.bat`

**Time Required:**
- Registration: 5-10 minutes
- Download: 30-60 minutes (web installer)
- Installation: 15-30 minutes
- **Total**: ~1-2 hours

**Space Required:**
- Download: 50 MB (web installer) or 6-8 GB (full)
- Installation: 5-15 GB (depending on components)

---

You're now ready to build Linkbar from command line!
