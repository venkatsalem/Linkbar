# Linkbar

Windows desktop toolbar that docks to any screen edge. Displays shortcut icons from a working directory.

## Requirements

- Windows 11 64-bit
- .NET 8.0 SDK

## Build

```
dotnet build
```

## Publish (single-file exe)

```
dotnet publish -c Release -p:SelfContained=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=false
```

## Features

- AppBar docking to any screen edge (top, bottom, left, right)
- Loads .lnk, .url, .website files from a working directory
- Sharp icons via IShellItemImageFactory (no shortcut overlay arrows)
- Drag-to-reorder icons within the bar
- Drag and drop files onto the bar to add shortcuts
- Auto-hide with hotkey toggle
- Context menu for settings (position, icon size, lock, sort, always on top)
- FileSystemWatcher for live directory monitoring
- INI-based settings compatible with original Linkbar .lbr format
- No internet calls, no external references

## Usage

Run `Linkbar.exe`. It looks for `.lbr` settings files in:
1. `Shared bars\` next to the exe
2. `%AppData%\Linkbar\User bars\`
3. Creates `default.lbr` if none found

Right-click the bar for options. Drag icons to reorder. Drop files to add shortcuts.

## License

See [LICENSE](LICENSE).
