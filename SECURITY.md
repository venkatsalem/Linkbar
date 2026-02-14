# Security Policy for Linkbar

## Overview
This version of Linkbar has been modified to be fully secure, firewall-safe, and suitable for use in restricted environments including air-gapped systems.

## Security Measures

### Removed External References
- **All URLs and email addresses** removed from source code
- **No web links** in the application interface
- **No email functionality** - SendShellEmail function removed
- **No external dependencies** that require internet access

### Network Isolation
- **No network API calls** - The application does not use WinINet, WinHTTP, or socket APIs
- **No outbound connections** - The application will never attempt to connect to the internet
- **No telemetry** - No data collection or reporting features
- **No auto-updates** - Application will not attempt to download updates

### Firewall Safety
- **No firewall exceptions needed** - Application operates entirely locally
- **Compatible with restrictive firewalls** - No blocked ports or protocols
- **Safe for air-gapped systems** - No internet dependency whatsoever

### Local Operation Only
All features operate locally:
- **File system access** - Creates shortcuts, reads .lnk/.url/.website files
- **Shell integration** - Uses Windows shell for file operations
- **Clipboard operations** - Standard Windows clipboard functionality
- **Windows API integration** - Uses only local Windows APIs

### No External Communications
The application does not communicate with:
- External websites
- Email servers
- Update servers
- Telemetry services
- Analytics services
- Cloud services

## Windows 11 64-bit Compatibility

### Platform Support
- **Target Platform**: Win64 (64-bit Windows)
- **Compatible OS**: Windows Vista, 7, 8, 8.1, 10, and 11
- **DPI-Aware**: Properly handles high-DPI displays
- **Dark Mode**: Supports Windows 11 dark theme

### Windows 11 Security Features
- **UAC Compatible**: No administrator privileges required
- **SmartScreen**: No issues with Windows Defender SmartScreen
- **Windows Hello**: Compatible with Windows Hello authentication
- **Secure Boot**: No kernel drivers or system modifications

## Code Review

### Removed Functions
- `SendShellEmail()` - Email sending functionality removed
- URL constants (URL_WEB, URL_EMAIL, URL_GITHUB, URL_WINDOWS_HOTKEY) - Removed
- Link labels in settings form - Removed

### Verified Safe APIs
The application only uses Windows APIs for:
- Window management
- File operations
- Shell operations
- Graphics and theming
- Input handling
- Process creation

### No Third-Party Network Libraries
The application does not include or use:
- HTTP/HTTPS libraries
- Socket libraries
- Network protocol implementations
- Remote procedure calls
- Web browser controls

## Build Verification

### Source Code Verification
All source files have been reviewed to ensure:
- No embedded URLs
- No hardcoded IP addresses
- No network communication code
- No email sending code

### Build Process
- Requires only local Delphi installation
- No internet connection required for building
- No external package downloads needed
- All dependencies included in repository

## Deployment Security

### Distribution
- **Offline distribution**: Can be distributed without internet
- **No online activation**: No activation or licensing servers
- **No DRM**: No digital rights management requiring internet

### Installation
- **No installer required**: Can run as portable executable
- **No registry modifications**: Minimal registry usage only
- **No system services**: Runs as user-space application
- **No kernel drivers**: No driver installation

### Runtime Security
- **No elevation**: Runs at user privilege level
- **No persistent connections**: No background network activity
- **No data exfiltration**: All data remains on local system

## Audit Trail

### Modifications Made
1. Removed URL constants from `Linkbar.Consts.pas`
2. Removed `SendShellEmail()` function from `Linkbar.Shell.pas`
3. Removed URL comments from source files
4. Removed link labels from Settings Form
5. Updated documentation to remove external references
6. Verified no network API calls remain

### Files Modified
- `src/Linkbar.Consts.pas`
- `src/Linkbar.Shell.pas`
- `src/Linkbar.SettingsForm.pas`
- `src/Linkbar.SettingsForm.dfm`
- `src/Linkbar.Theme.pas`
- `src/Linkbar.Undoc.pas`
- `src/Linkbar.ExceptionDialog.pas`
- `README.md`
- `exe/README.txt`
- `ico/README.txt`

## Compliance

This version of Linkbar is suitable for:
- **Government networks** with strict security policies
- **Corporate environments** with air-gapped systems
- **Military networks** requiring complete isolation
- **Industrial control systems** (ICS) environments
- **Healthcare systems** with HIPAA requirements

## Security Best Practices

### Recommended Use
1. **Run as standard user** - No administrator privileges needed
2. **Keep Windows updated** - Ensure OS security patches are current
3. **Use Windows Defender** - Default antivirus is sufficient
4. **Restrict execution** - Only run from trusted locations

### Additional Hardening (Optional)
For maximum security:
- Run in AppLocker controlled environment
- Use Windows Defender Application Control (WDAC)
- Enable Control Flow Guard (CFG)
- Use ASLR and DEP (enabled by default)
- Run in virtualized environment

## Reporting Security Issues

If you discover any security concerns:
1. Verify the issue is not related to the removed internet features
2. Check that no external communication is occurring
3. Ensure the issue is a genuine security vulnerability
4. Report through appropriate channels for your environment

## Version Information
- **Modified Date**: 2025
- **Target Platform**: Windows 11 64-bit
- **Security Classification**: Firewall-safe, offline-only
- **Internet Required**: No
- **Network Access**: None
