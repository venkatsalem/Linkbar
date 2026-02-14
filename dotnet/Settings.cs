namespace Linkbar;

/// <summary>INI-file based settings, compatible with original Linkbar .lbr format</summary>
public sealed class LinkbarSettings
{
    private readonly string _filePath;

    // Panel
    public PanelEdge Edge { get; set; } = PanelEdge.Top;
    public bool AutoHide { get; set; }
    public bool AutoHideTransparency { get; set; }
    public int AutoShowDelay { get; set; }
    public int IconSize { get; set; } = 32;
    public int MarginX { get; set; } = 4;
    public int MarginY { get; set; } = 4;
    public int SeparatorWidth { get; set; } = 16;
    public bool LockBar { get; set; }
    public bool StayOnTop { get; set; } = true;
    public bool SortAlphabetically { get; set; }
    public bool TooltipShow { get; set; } = true;
    public string DirLinks { get; set; } = @".\links";
    public int MonitorNum { get; set; }
    public Color BackgroundColor { get; set; } = Color.FromArgb(0xD4, 0xD6, 0xDD);
    public bool UseBackgroundColor { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public bool UseTextColor { get; set; } = true;
    public int GlowSize { get; set; } = 0;
    public int CornerGap1 { get; set; }
    public int CornerGap2 { get; set; }

    // Hotkey
    public uint HotkeyModifiers { get; set; } = (uint)(NativeMethods.MOD_SHIFT | NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT);
    public uint HotkeyKey { get; set; } = (uint)Keys.L;

    public LinkbarSettings(string filePath) => _filePath = filePath;

    public static string GetRoamingFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Linkbar");
    }

    public void Load()
    {
        if (!File.Exists(_filePath)) return;
        var ini = new IniFile(_filePath);
        var s = "Main";

        DirLinks = ini.Read(s, "dirlinks", DirLinks);
        Edge = (PanelEdge)ini.ReadInt(s, "Edge", (int)Edge);
        AutoHide = ini.ReadBool(s, "autohide", AutoHide);
        AutoHideTransparency = ini.ReadBool(s, "autohidetransparency", AutoHideTransparency);
        AutoShowDelay = ini.ReadInt(s, "autoshowdelay", AutoShowDelay);
        IconSize = Math.Clamp(ini.ReadInt(s, "iconsize", IconSize), 16, 256);
        MarginX = Math.Clamp(ini.ReadInt(s, "marginx", MarginX), 0, 64);
        MarginY = Math.Clamp(ini.ReadInt(s, "marginy", MarginY), 0, 64);
        SeparatorWidth = Math.Clamp(ini.ReadInt(s, "separatorwidth", SeparatorWidth), 2, 256);
        LockBar = ini.ReadBool(s, "lockbar", LockBar);
        StayOnTop = ini.ReadBool(s, "stayontop", StayOnTop);
        SortAlphabetically = ini.ReadBool(s, "sortab", SortAlphabetically);
        TooltipShow = ini.ReadBool(s, "tooltipshow", TooltipShow);
        MonitorNum = ini.ReadInt(s, "monitornum", MonitorNum);
        UseBackgroundColor = ini.ReadBool(s, "usebgcolor", UseBackgroundColor);
        UseTextColor = ini.ReadBool(s, "usetxtcolor", UseTextColor);
        GlowSize = Math.Clamp(ini.ReadInt(s, "glowsize", GlowSize), 0, 16);
        CornerGap1 = ini.ReadInt(s, "corner1gapwidth", CornerGap1);
        CornerGap2 = ini.ReadInt(s, "corner2gapwidth", CornerGap2);

        var bgStr = ini.Read(s, "bgcolor", "");
        if (bgStr.StartsWith("$") && int.TryParse(bgStr[1..], System.Globalization.NumberStyles.HexNumber, null, out int bgVal))
            BackgroundColor = Color.FromArgb(bgVal);

        var txtStr = ini.Read(s, "txtcolor", "");
        if (txtStr.StartsWith("$") && int.TryParse(txtStr[1..], System.Globalization.NumberStyles.HexNumber, null, out int txtVal))
            TextColor = Color.FromArgb(255, Color.FromArgb(txtVal));
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir != null) Directory.CreateDirectory(dir);

        var ini = new IniFile(_filePath);
        var s = "Main";

        ini.Write(s, "dirlinks", DirLinks);
        ini.Write(s, "Edge", (int)Edge);
        ini.Write(s, "autohide", AutoHide);
        ini.Write(s, "autohidetransparency", AutoHideTransparency);
        ini.Write(s, "autoshowdelay", AutoShowDelay);
        ini.Write(s, "iconsize", IconSize);
        ini.Write(s, "marginx", MarginX);
        ini.Write(s, "marginy", MarginY);
        ini.Write(s, "separatorwidth", SeparatorWidth);
        ini.Write(s, "lockbar", LockBar);
        ini.Write(s, "stayontop", StayOnTop);
        ini.Write(s, "sortab", SortAlphabetically);
        ini.Write(s, "tooltipshow", TooltipShow);
        ini.Write(s, "monitornum", MonitorNum);
        ini.Write(s, "usebgcolor", UseBackgroundColor);
        ini.Write(s, "usetxtcolor", UseTextColor);
        ini.Write(s, "glowsize", GlowSize);
        ini.Write(s, "corner1gapwidth", CornerGap1);
        ini.Write(s, "corner2gapwidth", CornerGap2);
        ini.Write(s, "bgcolor", $"${BackgroundColor.ToArgb():X8}");
        ini.Write(s, "txtcolor", $"${(TextColor.ToArgb() & 0xFFFFFF):X6}");
        ini.Save();
    }

    public bool IsValid() => Directory.Exists(DirLinks);
}

public enum PanelEdge { Left = 0, Top = 1, Right = 2, Bottom = 3 }

/// <summary>Simple INI file reader/writer</summary>
public sealed class IniFile
{
    private readonly string _path;
    private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.OrdinalIgnoreCase);

    public IniFile(string path)
    {
        _path = path;
        if (!File.Exists(path)) return;
        string? section = null;
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1];
                if (!_data.ContainsKey(section)) _data[section] = new(StringComparer.OrdinalIgnoreCase);
            }
            else if (section != null)
            {
                var eq = line.IndexOf('=');
                if (eq > 0) _data[section][line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }
        }
    }

    public string Read(string section, string key, string def = "")
    {
        if (_data.TryGetValue(section, out var s) && s.TryGetValue(key, out var v)) return v;
        return def;
    }

    public int ReadInt(string section, string key, int def = 0)
        => int.TryParse(Read(section, key), out int v) ? v : def;

    public bool ReadBool(string section, string key, bool def = false)
    {
        var s = Read(section, key).ToLowerInvariant();
        if (s == "1" || s == "true") return true;
        if (s == "0" || s == "false") return false;
        return def;
    }

    public void Write(string section, string key, object value)
    {
        if (!_data.ContainsKey(section)) _data[section] = new(StringComparer.OrdinalIgnoreCase);
        _data[section][key] = value switch
        {
            bool b => b ? "1" : "0",
            _ => value.ToString() ?? ""
        };
    }

    public void Save()
    {
        using var w = new StreamWriter(_path);
        foreach (var (section, entries) in _data)
        {
            w.WriteLine($"[{section}]");
            foreach (var (key, val) in entries) w.WriteLine($"{key}={val}");
            w.WriteLine();
        }
    }
}
