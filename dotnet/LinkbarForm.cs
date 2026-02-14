using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace Linkbar;

/// <summary>Main toolbar window - docks to screen edge as AppBar</summary>
public sealed class LinkbarForm : Form
{
    private const int GRIP = 12;
    private const int APPBAR_CALLBACK = NativeMethods.WM_USER + 100;
    private const int HOTKEY_ID = 1;
    private const int AUTOHIDE_SIZE = 2;

    private readonly LinkbarSettings _settings;
    private readonly LinkItemCollection _items = new();
    private readonly Timer _autoHideTimer = new();
    private readonly ToolTip _tooltip = new() { InitialDelay = 400, ReshowDelay = 200 };

    private const int DRAG_THRESHOLD = 5;

    private int _hotIndex = -1;
    private int _pressedIndex = -1;
    private bool _appBarRegistered;
    private bool _autoHidden;
    private Point _mouseDownPos;
    private bool _mouseLeftDown;
    private uint _taskbarCreatedMsg;
    private Bitmap? _backBuffer;
    private FileSystemWatcher? _watcher;
    private readonly Timer _reloadTimer = new() { Interval = 500 }; // debounce FS changes

    // Drag-to-reorder state
    private bool _dragging;
    private int _dragFromIndex = -1;
    private int _dragInsertIndex = -1;

    public LinkbarForm(LinkbarSettings settings)
    {
        _settings = settings;
        _settings.Load();

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = _settings.StayOnTop;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(0xD4, 0xD6, 0xDD);

        _autoHideTimer.Interval = 300;
        _autoHideTimer.Tick += (_, _) => { _autoHideTimer.Stop(); DoAutoHide(); };

        // Drag & drop support: accept files dropped onto the bar
        AllowDrop = true;

        // Debounced reload timer for FileSystemWatcher
        _reloadTimer.Tick += (_, _) => { _reloadTimer.Stop(); ReloadItems(); };
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style = NativeMethods.WS_POPUP;
            cp.ExStyle = NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_TOPMOST;
            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _taskbarCreatedMsg = NativeMethods.RegisterWindowMessage("TaskbarCreated");

        // Resolve working directory
        var workDir = _settings.DirLinks;
        if (!Path.IsPathRooted(workDir))
            workDir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? AppContext.BaseDirectory, workDir);
        if (!Directory.Exists(workDir))
            Directory.CreateDirectory(workDir);

        _items.LoadFromDirectory(workDir, _settings.IconSize, Path.Combine(workDir, "list"));
        if (_settings.SortAlphabetically) _items.Sort();

        RegisterAppBar();
        PositionBar();

        // Register hotkey
        if (_settings.AutoHide)
            NativeMethods.RegisterHotKey(Handle, HOTKEY_ID, _settings.HotkeyModifiers, _settings.HotkeyKey);

        // Watch working directory for changes
        StartFileWatcher(workDir);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        NativeMethods.UnregisterHotKey(Handle, HOTKEY_ID);
        UnregisterAppBar();
        StopFileWatcher();
        _reloadTimer.Dispose();
        _items.Dispose();
        _backBuffer?.Dispose();
        _tooltip.Dispose();
        _autoHideTimer.Dispose();
        base.OnFormClosed(e);
    }

    // ── AppBar Registration ──
    private void RegisterAppBar()
    {
        if (_appBarRegistered) return;
        var abd = NewABD();
        abd.uCallbackMessage = APPBAR_CALLBACK;
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref abd);
        _appBarRegistered = true;
    }

    private void UnregisterAppBar()
    {
        if (!_appBarRegistered) return;
        var abd = NewABD();
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_REMOVE, ref abd);
        _appBarRegistered = false;
    }

    private NativeMethods.APPBARDATA NewABD()
    {
        var abd = new NativeMethods.APPBARDATA();
        abd.cbSize = Marshal.SizeOf(abd);
        abd.hWnd = Handle;
        return abd;
    }

    private void PositionBar()
    {
        var screen = _settings.MonitorNum < Screen.AllScreens.Length
            ? Screen.AllScreens[_settings.MonitorNum] : Screen.PrimaryScreen!;
        var wa = _settings.AutoHide ? screen.Bounds : screen.WorkingArea;
        bool vertical = _settings.Edge is PanelEdge.Left or PanelEdge.Right;

        // Calculate needed size
        int itemCount = Math.Max(1, _items.Count);
        int btnSize = _settings.IconSize + _settings.MarginX * 2;
        int barThickness = btnSize;
        int barLength = vertical ? wa.Height : wa.Width;

        // Update item layout
        _items.UpdateLayout(vertical,
            new Size(btnSize, btnSize),
            _settings.SeparatorWidth, GRIP, barLength, barLength);

        // Compute total content length
        int contentLen = GRIP;
        foreach (var item in _items)
        {
            if (vertical) contentLen = Math.Max(contentLen, item.Bounds.Bottom);
            else contentLen = Math.Max(contentLen, item.Bounds.Right);
        }
        contentLen += GRIP;

        Rectangle bounds;
        switch (_settings.Edge)
        {
            case PanelEdge.Top:
                bounds = new Rectangle(wa.Left, wa.Top, wa.Width, barThickness);
                break;
            case PanelEdge.Bottom:
                bounds = new Rectangle(wa.Left, wa.Bottom - barThickness, wa.Width, barThickness);
                break;
            case PanelEdge.Left:
                bounds = new Rectangle(wa.Left, wa.Top, barThickness, wa.Height);
                break;
            case PanelEdge.Right:
                bounds = new Rectangle(wa.Right - barThickness, wa.Top, barThickness, wa.Height);
                break;
            default:
                bounds = new Rectangle(wa.Left, wa.Top, wa.Width, barThickness);
                break;
        }

        // Set AppBar position
        if (_appBarRegistered && !_settings.AutoHide)
        {
            var abd = NewABD();
            abd.uEdge = (uint)_settings.Edge;
            abd.rc = NativeMethods.RECT.FromRectangle(screen.Bounds);
            NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref abd);

            switch (_settings.Edge)
            {
                case PanelEdge.Top: abd.rc.Bottom = abd.rc.Top + barThickness; break;
                case PanelEdge.Bottom: abd.rc.Top = abd.rc.Bottom - barThickness; break;
                case PanelEdge.Left: abd.rc.Right = abd.rc.Left + barThickness; break;
                case PanelEdge.Right: abd.rc.Left = abd.rc.Right - barThickness; break;
            }
            NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref abd);
            bounds = abd.rc.ToRectangle();
        }

        SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        _backBuffer?.Dispose();
        _backBuffer = null;
        Invalidate();
    }

    // ── Painting ──
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Background
        var bgColor = Color.FromArgb(0xD4, 0xD6, 0xDD);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, ClientRectangle);

        // Subtle border line
        bool vertical = _settings.Edge is PanelEdge.Left or PanelEdge.Right;
        using var borderPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1);
        switch (_settings.Edge)
        {
            case PanelEdge.Top: g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1); break;
            case PanelEdge.Bottom: g.DrawLine(borderPen, 0, 0, Width, 0); break;
            case PanelEdge.Left: g.DrawLine(borderPen, Width - 1, 0, Width - 1, Height); break;
            case PanelEdge.Right: g.DrawLine(borderPen, 0, 0, 0, Height); break;
        }

        // Items
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (item.IsSeparator)
            {
                DrawSeparator(g, item.Bounds, vertical);
                continue;
            }

            // Hover / pressed background
            if (i == _pressedIndex)
            {
                using var b = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                g.FillRectangle(b, item.Bounds);
            }
            else if (i == _hotIndex)
            {
                using var b = new SolidBrush(Color.FromArgb(35, 255, 255, 255));
                g.FillRectangle(b, item.Bounds);
            }

            // Dim the item being dragged
            if (_dragging && i == _dragFromIndex)
            {
                if (item.IconBitmap != null)
                {
                    int iconSize = _settings.IconSize;
                    int ix = item.Bounds.X + (item.Bounds.Width - iconSize) / 2;
                    int iy = item.Bounds.Y + (item.Bounds.Height - iconSize) / 2;
                    using var attr = new System.Drawing.Imaging.ImageAttributes();
                    float[][] matrixItems = {
                        new float[] {1,0,0,0,0},
                        new float[] {0,1,0,0,0},
                        new float[] {0,0,1,0,0},
                        new float[] {0,0,0,0.3f,0},
                        new float[] {0,0,0,0,1}
                    };
                    attr.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(matrixItems));
                    g.DrawImage(item.IconBitmap, new Rectangle(ix, iy, iconSize, iconSize),
                        0, 0, item.IconBitmap.Width, item.IconBitmap.Height, GraphicsUnit.Pixel, attr);
                }
                continue;
            }

            // Icon — draw bitmap at 1:1, no scaling needed (extracted at exact size)
            if (item.IconBitmap != null)
            {
                int iconSize = _settings.IconSize;
                int ix = item.Bounds.X + (item.Bounds.Width - iconSize) / 2;
                int iy = item.Bounds.Y + (item.Bounds.Height - iconSize) / 2;
                g.DrawImage(item.IconBitmap, ix, iy, iconSize, iconSize);
            }
        }

        // Draw drag insertion indicator
        if (_dragging && _dragInsertIndex >= 0 && _dragInsertIndex < _items.Count)
        {
            using var indicatorPen = new Pen(Color.FromArgb(0x33, 0x55, 0x88), 3);
            var target = _items[_dragInsertIndex].Bounds;
            if (vertical)
            {
                int y = target.Top;
                g.DrawLine(indicatorPen, target.Left + 2, y, target.Right - 2, y);
            }
            else
            {
                int x = target.Left;
                g.DrawLine(indicatorPen, x, target.Top + 2, x, target.Bottom - 2);
            }
        }
    }

    private void DrawSeparator(Graphics g, Rectangle bounds, bool vertical)
    {
        using var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1);
        if (vertical)
            g.DrawLine(pen, bounds.Left + 4, bounds.Top + bounds.Height / 2, bounds.Right - 4, bounds.Top + bounds.Height / 2);
        else
            g.DrawLine(pen, bounds.Left + bounds.Width / 2, bounds.Top + 4, bounds.Left + bounds.Width / 2, bounds.Bottom - 4);
    }

    // ── Mouse Interaction ──
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_autoHidden) return;

        // Check if we should start dragging
        if (_mouseLeftDown && !_dragging && _pressedIndex >= 0 && !_settings.LockBar)
        {
            int dx = Math.Abs(e.X - _mouseDownPos.X);
            int dy = Math.Abs(e.Y - _mouseDownPos.Y);
            if (dx > DRAG_THRESHOLD || dy > DRAG_THRESHOLD)
            {
                _dragging = true;
                _dragFromIndex = _pressedIndex;
                _tooltip.Hide(this);
                Cursor = Cursors.SizeAll;
            }
        }

        if (_dragging)
        {
            // Compute insertion index from mouse position
            _dragInsertIndex = CalcInsertIndex(e.Location);
            Invalidate();
            return;
        }

        int idx = _items.HitTest(e.Location);
        if (idx != _hotIndex)
        {
            _hotIndex = idx;
            Invalidate();

            _tooltip.Hide(this);
            if (idx >= 0 && !_items[idx].IsSeparator && _settings.TooltipShow)
                _tooltip.Show(_items[idx].DisplayName, this, e.X + 16, e.Y + 16, 3000);
        }

        // Cancel auto-hide while mouse is on the bar
        _autoHideTimer.Stop();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_autoHidden) { DoAutoShow(); return; }
        if (e.Button == MouseButtons.Left)
        {
            _mouseLeftDown = true;
            _mouseDownPos = e.Location;
            _pressedIndex = _items.HitTest(e.Location);
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && _mouseLeftDown)
        {
            _mouseLeftDown = false;

            if (_dragging)
            {
                // Finish drag-to-reorder
                _dragging = false;
                Cursor = Cursors.Default;

                int insertAt = CalcInsertIndex(e.Location);
                if (_dragFromIndex >= 0 && insertAt >= 0 && insertAt != _dragFromIndex)
                {
                    _items.Move(_dragFromIndex, insertAt);
                    // Save new order
                    var workDir = ResolveWorkDir();
                    _items.SaveOrder(Path.Combine(workDir, "list"));
                    PositionBar();
                }

                _dragFromIndex = -1;
                _dragInsertIndex = -1;
                _pressedIndex = -1;
                Invalidate();
                return;
            }

            int idx = _items.HitTest(e.Location);
            if (idx >= 0 && idx == _pressedIndex && !_items[idx].IsSeparator)
            {
                _items[idx].Execute();
            }
            _pressedIndex = -1;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_dragging)
        {
            // Cancel drag if mouse leaves the bar
            _dragging = false;
            _dragFromIndex = -1;
            _dragInsertIndex = -1;
            _pressedIndex = -1;
            Cursor = Cursors.Default;
        }
        if (_hotIndex != -1) { _hotIndex = -1; }
        _tooltip.Hide(this);
        Invalidate();

        if (_settings.AutoHide && !_autoHidden)
            _autoHideTimer.Start();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _autoHideTimer.Stop();
        if (_autoHidden) DoAutoShow();
    }

    /// <summary>Calculate the insertion index for drag-to-reorder based on mouse position</summary>
    private int CalcInsertIndex(Point pt)
    {
        if (_items.Count == 0) return 0;
        bool vertical = _settings.Edge is PanelEdge.Left or PanelEdge.Right;

        for (int i = 0; i < _items.Count; i++)
        {
            var b = _items[i].Bounds;
            if (vertical)
            {
                int mid = b.Top + b.Height / 2;
                if (pt.Y < mid) return i;
            }
            else
            {
                int mid = b.Left + b.Width / 2;
                if (pt.X < mid) return i;
            }
        }
        return _items.Count - 1;
    }

    // ── Context Menu ──
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Right)
        {
            int idx = _items.HitTest(e.Location);
            if (idx >= 0 && !_items[idx].IsSeparator)
            {
                // Item context menu - open file location
                ShowItemContextMenu(idx, PointToScreen(e.Location));
            }
            else
            {
                ShowBarContextMenu(PointToScreen(e.Location));
            }
        }
    }

    private void ShowItemContextMenu(int idx, Point screenPt)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => _items[idx].Execute());
        menu.Items.Add("Open file location", null, (_, _) =>
        {
            var dir = Path.GetDirectoryName(_items[idx].FilePath);
            if (dir != null) NativeMethods.ShellExecute(IntPtr.Zero, "open", dir, null, null, NativeMethods.SW_SHOWNORMAL);
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Delete shortcut", null, (_, _) =>
        {
            if (MessageBox.Show($"Delete '{_items[idx].DisplayName}'?", "Linkbar",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try { File.Delete(_items[idx].FilePath); } catch { }
                _items.RemoveAt(idx);
                _hotIndex = -1;
                PositionBar();
            }
        });
        menu.Show(screenPt);
    }

    private void ShowBarContextMenu(Point screenPt)
    {
        var menu = new ContextMenuStrip();

        // Position submenu
        var posMenu = new ToolStripMenuItem("Position");
        foreach (PanelEdge edge in Enum.GetValues<PanelEdge>())
        {
            var e = edge;
            var mi = new ToolStripMenuItem(e.ToString()) { Checked = _settings.Edge == e };
            mi.Click += (_, _) => { _settings.Edge = e; _settings.Save(); PositionBar(); };
            posMenu.DropDownItems.Add(mi);
        }
        menu.Items.Add(posMenu);

        // Icon size submenu
        var sizeMenu = new ToolStripMenuItem("Icon size");
        foreach (var sz in new[] { 16, 24, 32, 48, 64 })
        {
            var s = sz;
            var mi = new ToolStripMenuItem($"{s}px") { Checked = _settings.IconSize == s };
            mi.Click += (_, _) =>
            {
                _settings.IconSize = s;
                _settings.Save();
                ReloadItems();
            };
            sizeMenu.DropDownItems.Add(mi);
        }
        menu.Items.Add(sizeMenu);

        menu.Items.Add(new ToolStripSeparator());

        var workDir = ResolveWorkDir();
        menu.Items.Add("Open working directory", null, (_, _) =>
            NativeMethods.ShellExecute(IntPtr.Zero, "open", workDir, null, null, NativeMethods.SW_SHOWNORMAL));

        var lockItem = new ToolStripMenuItem("Lock the linkbar") { Checked = _settings.LockBar };
        lockItem.Click += (_, _) => { _settings.LockBar = !_settings.LockBar; _settings.Save(); };
        menu.Items.Add(lockItem);

        var sortItem = new ToolStripMenuItem("Sort alphabetically") { Checked = _settings.SortAlphabetically };
        sortItem.Click += (_, _) =>
        {
            _settings.SortAlphabetically = !_settings.SortAlphabetically;
            _settings.Save();
            if (_settings.SortAlphabetically) { _items.Sort(); PositionBar(); }
        };
        menu.Items.Add(sortItem);

        var ahItem = new ToolStripMenuItem("Auto-hide") { Checked = _settings.AutoHide };
        ahItem.Click += (_, _) => { _settings.AutoHide = !_settings.AutoHide; _settings.Save(); PositionBar(); };
        menu.Items.Add(ahItem);

        var topItem = new ToolStripMenuItem("Always on top") { Checked = _settings.StayOnTop };
        topItem.Click += (_, _) =>
        {
            _settings.StayOnTop = !_settings.StayOnTop;
            TopMost = _settings.StayOnTop;
            _settings.Save();
        };
        menu.Items.Add(topItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Close", null, (_, _) => Close());

        menu.Show(screenPt);
    }

    // ── AutoHide ──
    private void DoAutoHide()
    {
        if (!_settings.AutoHide || _autoHidden) return;
        _autoHidden = true;

        var screen = GetCurrentScreen();
        switch (_settings.Edge)
        {
            case PanelEdge.Top: SetBounds(Left, screen.Bounds.Top, Width, AUTOHIDE_SIZE); break;
            case PanelEdge.Bottom: SetBounds(Left, screen.Bounds.Bottom - AUTOHIDE_SIZE, Width, AUTOHIDE_SIZE); break;
            case PanelEdge.Left: SetBounds(screen.Bounds.Left, Top, AUTOHIDE_SIZE, Height); break;
            case PanelEdge.Right: SetBounds(screen.Bounds.Right - AUTOHIDE_SIZE, Top, AUTOHIDE_SIZE, Height); break;
        }
        Invalidate();
    }

    private void DoAutoShow()
    {
        if (!_autoHidden) return;
        _autoHidden = false;
        PositionBar();
    }

    private Screen GetCurrentScreen()
    {
        return _settings.MonitorNum < Screen.AllScreens.Length
            ? Screen.AllScreens[_settings.MonitorNum] : Screen.PrimaryScreen!;
    }

    private string ResolveWorkDir()
    {
        var wd = _settings.DirLinks;
        if (!Path.IsPathRooted(wd))
            wd = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? ".", wd);
        return wd;
    }

    private void ReloadItems()
    {
        var workDir = ResolveWorkDir();
        _items.LoadFromDirectory(workDir, _settings.IconSize, Path.Combine(workDir, "list"));
        if (_settings.SortAlphabetically) _items.Sort();
        PositionBar();
    }

    // ── FileSystemWatcher ──
    private void StartFileWatcher(string directory)
    {
        try
        {
            _watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += (_, _) => ScheduleReload();
            _watcher.Changed += OnFileChanged;
        }
        catch { /* directory may not exist or be inaccessible */ }
    }

    private void StopFileWatcher()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLowerInvariant();
        if (ext is ".lnk" or ".url" or ".website" || Path.GetFileName(e.FullPath) == "list")
            ScheduleReload();
    }

    private void ScheduleReload()
    {
        if (InvokeRequired)
        {
            try { BeginInvoke(ScheduleReload); } catch { }
            return;
        }
        // Restart debounce timer
        _reloadTimer.Stop();
        _reloadTimer.Start();
    }

    // ── Drag & Drop ──
    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
        base.OnDragDrop(e);
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;

        var workDir = ResolveWorkDir();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".lnk" or ".url" or ".website")
            {
                // Copy the shortcut file into the working directory
                var destName = Path.GetFileName(file);
                var destPath = Path.Combine(workDir, destName);
                try
                {
                    File.Copy(file, destPath, overwrite: false);
                }
                catch { /* file may already exist */ }
            }
            else
            {
                // Create a .lnk shortcut to the dropped file/folder
                try
                {
                    CreateShortcut(workDir, file);
                }
                catch { }
            }
        }
        // FileSystemWatcher will trigger reload, but do an immediate reload too
        ReloadItems();
    }

    private static void CreateShortcut(string workDir, string targetPath)
    {
        var name = Path.GetFileNameWithoutExtension(targetPath);
        var lnkPath = Path.Combine(workDir, name + ".lnk");
        // Avoid overwriting
        int n = 1;
        while (File.Exists(lnkPath))
            lnkPath = Path.Combine(workDir, $"{name} ({n++}).lnk");

        var link = (NativeMethods.IShellLinkW)new NativeMethods.ShellLink();
        link.SetPath(targetPath);
        link.SetWorkingDirectory(Path.GetDirectoryName(targetPath) ?? "");
        ((System.Runtime.InteropServices.ComTypes.IPersistFile)link).Save(lnkPath, true);
    }

    // ── WndProc for AppBar messages and hotkeys ──
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == APPBAR_CALLBACK)
        {
            switch ((int)m.WParam)
            {
                case NativeMethods.ABN_POSCHANGED:
                    PositionBar();
                    break;
                case NativeMethods.ABN_FULLSCREENAPP:
                    if ((int)m.LParam != 0)
                        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                    else if (_settings.StayOnTop)
                        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                    break;
            }
            return;
        }

        if (m.Msg == NativeMethods.WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
        {
            if (_autoHidden) DoAutoShow(); else DoAutoHide();
            return;
        }

        if (m.Msg == NativeMethods.WM_DISPLAYCHANGE)
        {
            PositionBar();
            return;
        }

        if (_taskbarCreatedMsg != 0 && m.Msg == (int)_taskbarCreatedMsg)
        {
            UnregisterAppBar();
            RegisterAppBar();
            PositionBar();
            return;
        }

        base.WndProc(ref m);
    }

    // ── Keyboard ──
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            if (_settings.AutoHide) DoAutoHide();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
