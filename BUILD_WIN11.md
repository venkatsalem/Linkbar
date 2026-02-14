# Linkbar - Secure Build Instructions for Windows 11 64-bit

## Overview
Linkbar is a free source code desktop toolbar for Windows. This version has been modified to be secure and firewall-safe with no internet calls or external references.

## Requirements
- **Embarcadero Delphi 10.3 Community Edition or higher** (or compatible Delphi version)
- Windows 11 64-bit (or Windows Vista+)
- No internet connection required for building

## Security Features
- All external URLs and email addresses removed
- No network or internet calls
- No telemetry or data collection
- Firewall-safe design
- Offline operation only

## Build Instructions

### Step 1: Open the Project
1. Launch Embarcadero Delphi
2. Open the project file: `src\Linkbar.dproj`
3. Ensure the target platform is set to **Win64**
   - In the Project Manager, right-click on "Target Platforms"
   - Select "Win64" if not already active
   - The project defaults to Win64 platform

### Step 2: Configure Build Settings
1. Go to Project > Options
2. Ensure the following settings:
   - **Target Platform**: Win64
   - **Configuration**: Release (for production build) or Debug (for development)
   - No additional packages or runtime packages required

### Step 3: Build the Project
1. Press **F9** or click **Run > Run** to build and run
2. Or press **Ctrl+F9** or click **Project > Compile Linkbar** to build without running
3. The executable will be created in the `src` directory

### Step 4: Post-Build
The build process automatically creates a copy of the executable with the platform suffix:
- Debug build: `LinkbarWin64D.exe`
- Release build: `LinkbarWin64.exe`

## Windows 11 Compatibility
This application is fully compatible with Windows 11 64-bit:
- Supports Windows 11 dark mode
- DPI-aware for high-resolution displays
- Windows 11 taskbar integration
- No compatibility issues with Windows 11 security features

## Offline Installation
To distribute or use Linkbar offline:
1. Copy the built executable (`LinkbarWin64.exe`)
2. Copy the `Locales` folder from `exe\Locales\`
3. Place these in your desired installation directory
4. Run the executable - no internet connection required

## Configuration Files
- Profiles are stored as `.lbr` files
- Language files are in `Locales\` folder
- No registry entries are required for basic operation
- All settings stored locally in user's AppData or application folder

## Security Verification
To verify the application is secure:
1. No external URLs in the code
2. No email functionality
3. No network API calls
4. No telemetry or analytics
5. All communications are local only (file system, clipboard, shell)

## Firewall Settings
Since Linkbar makes no network connections:
- No firewall exceptions needed
- No outbound connections required
- Safe to run in restricted network environments
- Compatible with air-gapped systems

## License
MIT License - See LICENSE file for details

## Troubleshooting
- If build fails, ensure Delphi is properly installed and Win64 platform is selected
- For Windows 11 compatibility issues, ensure you have the latest Delphi updates
- All features work offline - if something seems to require internet, it's a bug to report
