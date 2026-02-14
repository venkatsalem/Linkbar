using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Linkbar;

/// <summary>Represents a shortcut item on the toolbar</summary>
public sealed class LinkItem
{
    public string FilePath { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public string Arguments { get; set; } = "";
    public string WorkingDir { get; set; } = "";
    public string IconPath { get; set; } = "";
    public int IconIndex { get; set; }
    public Bitmap? IconBitmap { get; set; }
    public Rectangle Bounds { get; set; }
    public bool IsSeparator { get; set; }

    public void Execute()
    {
        if (IsSeparator) return;
        try
        {
            if (File.Exists(FilePath))
                NativeMethods.ShellExecute(IntPtr.Zero, "open", FilePath, null, null, NativeMethods.SW_SHOWNORMAL);
        }
        catch { /* silently fail */ }
    }

    public void Dispose()
    {
        IconBitmap?.Dispose();
        IconBitmap = null;
    }

    /// <summary>Load a .lnk, .url, or .website file</summary>
    public static LinkItem? LoadFromFile(string filePath, int iconSize)
    {
        if (!File.Exists(filePath)) return null;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext != ".lnk" && ext != ".url" && ext != ".website") return null;

        var item = new LinkItem { FilePath = filePath };

        // Try to resolve .lnk target
        if (ext == ".lnk")
        {
            try
            {
                var link = (NativeMethods.IShellLinkW)new NativeMethods.ShellLink();
                ((System.Runtime.InteropServices.ComTypes.IPersistFile)link).Load(filePath, 0);
                try { link.Resolve(IntPtr.Zero, NativeMethods.SLR_NO_UI | NativeMethods.SLR_NOSEARCH); } catch { }

                var sb = new System.Text.StringBuilder(260);
                try { link.GetPath(sb, sb.Capacity, IntPtr.Zero, NativeMethods.SLGP_RAWPATH); item.TargetPath = sb.ToString(); } catch { }

                sb.Clear();
                try { link.GetDescription(sb, sb.Capacity); } catch { }

                sb.Clear();
                try { link.GetWorkingDirectory(sb, sb.Capacity); item.WorkingDir = sb.ToString(); } catch { }

                sb.Clear();
                try { link.GetArguments(sb, sb.Capacity); item.Arguments = sb.ToString(); } catch { }

                sb.Clear();
                try
                {
                    link.GetIconLocation(sb, sb.Capacity, out int idx);
                    item.IconPath = sb.ToString();
                    item.IconIndex = idx;
                }
                catch { }
            }
            catch { }
        }

        // Display name from file name (without extension for .lnk)
        item.DisplayName = ext == ".lnk"
            ? Path.GetFileNameWithoutExtension(filePath)
            : Path.GetFileName(filePath);

        // Extract icon at exact pixel size via IShellItemImageFactory (no overlay, sharp)
        item.IconBitmap = ExtractShellBitmap(filePath, iconSize);

        return item;
    }

    /// <summary>Load a separator</summary>
    public static LinkItem CreateSeparator() => new() { IsSeparator = true, DisplayName = "|", FilePath = "|" };

    /// <summary>
    /// Extract icon via IShellItemImageFactory.GetImage — returns a bitmap at the
    /// exact requested pixel size with no shortcut overlay arrow, matching the
    /// original Delphi Linkbar approach.
    /// </summary>
    private static Bitmap? ExtractShellBitmap(string filePath, int iconSize)
    {
        // Primary: IShellItemImageFactory — sharp, exact size, no overlay
        try
        {
            var iidFactory = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");
            NativeMethods.SHCreateItemFromParsingName(filePath, IntPtr.Zero, iidFactory, out object shellItemObj);

            if (shellItemObj is NativeMethods.IShellItemImageFactory factory)
            {
                var size = new NativeMethods.SIZE { Width = iconSize, Height = iconSize };
                int flags = NativeMethods.SIIGBF_ICONONLY | NativeMethods.SIIGBF_BIGGERSIZEOK;
                int hr = factory.GetImage(size, flags, out IntPtr hBitmap);
                if (hr == 0 && hBitmap != IntPtr.Zero)
                {
                    try
                    {
                        var bmp = HBitmapToBitmap(hBitmap, iconSize);
                        return bmp;
                    }
                    finally
                    {
                        NativeMethods.DeleteObject(hBitmap);
                    }
                }
            }
        }
        catch { }

        // Fallback: Icon.ExtractAssociatedIcon (no overlay control, may be blurry)
        try
        {
            var icon = Icon.ExtractAssociatedIcon(filePath);
            if (icon != null)
            {
                var bmp = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawIcon(icon, new Rectangle(0, 0, iconSize, iconSize));
                }
                icon.Dispose();
                return bmp;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Convert HBITMAP from IShellItemImageFactory to a managed 32bpp ARGB Bitmap,
    /// applying premultiplied alpha correction (required on Win 8.1+).
    /// </summary>
    private static Bitmap HBitmapToBitmap(IntPtr hBitmap, int iconSize)
    {
        var bmp = Image.FromHbitmap(hBitmap);

        // Image.FromHbitmap loses alpha. We need to read the raw bits directly.
        var bmInfo = new NativeMethods.BITMAP();
        NativeMethods.GetObject(hBitmap, Marshal.SizeOf<NativeMethods.BITMAP>(), ref bmInfo);

        if (bmInfo.bmBitsPixel == 32 && bmInfo.bmBits != IntPtr.Zero)
        {
            bmp.Dispose();

            // Create proper 32bpp ARGB bitmap from the raw pixel data
            int w = bmInfo.bmWidth;
            int h = bmInfo.bmHeight;
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var lockBits = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // HBITMAP is bottom-up; copy rows flipped
            int stride = bmInfo.bmWidthBytes;
            for (int y = 0; y < h; y++)
            {
                IntPtr srcRow = bmInfo.bmBits + (h - 1 - y) * stride;
                IntPtr dstRow = lockBits.Scan0 + y * lockBits.Stride;
                unsafe
                {
                    byte* src = (byte*)srcRow;
                    byte* dst = (byte*)dstRow;
                    for (int x = 0; x < w; x++)
                    {
                        byte b = src[0], g = src[1], r = src[2], a = src[3];
                        // Undo premultiply if needed (shell returns premultiplied on some OS versions)
                        if (a > 0 && a < 255)
                        {
                            dst[0] = (byte)Math.Min(255, b * 255 / a);
                            dst[1] = (byte)Math.Min(255, g * 255 / a);
                            dst[2] = (byte)Math.Min(255, r * 255 / a);
                            dst[3] = a;
                        }
                        else
                        {
                            dst[0] = b; dst[1] = g; dst[2] = r; dst[3] = a;
                        }
                        src += 4; dst += 4;
                    }
                }
            }
            result.UnlockBits(lockBits);
            return result;
        }

        // Non-32bpp fallback: just return what FromHbitmap gave us
        return bmp;
    }
}

/// <summary>Manages the ordered list of items and their layout</summary>
public sealed class LinkItemCollection : IDisposable, IEnumerable<LinkItem>
{
    private readonly List<LinkItem> _items = new();
    public int Count => _items.Count;
    public LinkItem this[int index] => _items[index];
    public IEnumerator<LinkItem> GetEnumerator() => _items.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public void Add(LinkItem item) => _items.Add(item);
    public void Insert(int index, LinkItem item) => _items.Insert(Math.Clamp(index, 0, _items.Count), item);
    public void RemoveAt(int index) { _items[index].Dispose(); _items.RemoveAt(index); }
    public void Clear() { foreach (var i in _items) i.Dispose(); _items.Clear(); }

    /// <summary>Move item from one index to another (for drag-to-reorder)</summary>
    public void Move(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _items.Count) return;
        toIndex = Math.Clamp(toIndex, 0, _items.Count - 1);
        if (fromIndex == toIndex) return;
        var item = _items[fromIndex];
        _items.RemoveAt(fromIndex);
        _items.Insert(toIndex, item);
    }

    public void Sort()
    {
        // Sort non-separator groups alphabetically, preserving separator positions
        int start = 0;
        for (int i = 0; i <= _items.Count; i++)
        {
            if (i == _items.Count || _items[i].IsSeparator)
            {
                if (i - start >= 2)
                    _items.Sort(start, i - start, Comparer<LinkItem>.Create(
                        (a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase)));
                start = i + 1;
            }
        }
    }

    public void UpdateLayout(bool vertical, Size buttonSize, int separatorWidth, int margin, int panelWidth, int panelHeight)
    {
        int x = vertical ? 0 : margin;
        int y = vertical ? margin : 0;

        foreach (var item in _items)
        {
            int w, h;
            if (item.IsSeparator)
            {
                w = vertical ? buttonSize.Width : separatorWidth;
                h = vertical ? separatorWidth : buttonSize.Height;
            }
            else
            {
                w = buttonSize.Width;
                h = buttonSize.Height;
            }

            item.Bounds = new Rectangle(x, y, w, h);

            if (vertical)
            {
                y += h;
                if (y + h > panelHeight) { y = margin; x += w; }
            }
            else
            {
                x += w;
                if (x + w > panelWidth) { x = margin; y += h; }
            }
        }
    }

    /// <summary>Load all shortcut files from a directory</summary>
    public void LoadFromDirectory(string dirPath, int iconSize, string? orderFile = null)
    {
        Clear();
        if (!Directory.Exists(dirPath)) return;

        var extensions = new[] { ".lnk", ".url", ".website" };
        var files = Directory.GetFiles(dirPath)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        // If order file exists, respect ordering
        List<string>? order = null;
        if (orderFile != null && File.Exists(orderFile))
        {
            order = File.ReadAllLines(orderFile).ToList();
        }

        if (order != null)
        {
            var fileMap = files.ToDictionary(f => Path.GetFileName(f), f => f, StringComparer.OrdinalIgnoreCase);
            foreach (var name in order)
            {
                if (name == "|")
                    _items.Add(LinkItem.CreateSeparator());
                else if (fileMap.TryGetValue(name, out var path))
                {
                    var item = LinkItem.LoadFromFile(path, iconSize);
                    if (item != null) _items.Add(item);
                    fileMap.Remove(name);
                }
            }
            // Add remaining new files
            foreach (var kv in fileMap)
            {
                var item = LinkItem.LoadFromFile(kv.Value, iconSize);
                if (item != null) _items.Add(item);
            }
        }
        else
        {
            foreach (var f in files.OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
            {
                var item = LinkItem.LoadFromFile(f, iconSize);
                if (item != null) _items.Add(item);
            }
        }
    }

    public void SaveOrder(string orderFile)
    {
        var lines = _items.Select(i => i.IsSeparator ? "|" : Path.GetFileName(i.FilePath));
        File.WriteAllLines(orderFile, lines);
    }

    public int HitTest(Point pt)
    {
        for (int i = 0; i < _items.Count; i++)
            if (_items[i].Bounds.Contains(pt)) return i;
        return -1;
    }

    public void Dispose() => Clear();
}
