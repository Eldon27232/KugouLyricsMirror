namespace KugouLyricsMirror;

internal sealed class BackdropForm : Form
{
    private bool _locked;

    public event EventHandler? LockedChanged;
    public bool IsLocked => _locked;

    public BackdropForm()
    {
        Text = "Lyrics Mirror Black Backdrop";
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        MinimumSize = new Size(40, 30);
        Size = new Size(600, 120);

        MouseDown += DragWindowMouseDown;

        var menu = new ContextMenuStrip();
        menu.Items.Add("隐藏黑色底板", null, (_, _) => Hide());
        menu.Items.Add("锁定 / 解锁", null, (_, _) => ApplyLocked(!_locked));
        ContextMenuStrip = menu;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WS_EX_NOACTIVATE = 0x08000000;
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE;
            return cp;
        }
    }

    public void ApplyConfig(Config config)
    {
        TopMost = true;
        ApplyLocked(config.BackdropLocked);

        var width = Math.Max(MinimumSize.Width, config.BackdropWidth);
        var height = Math.Max(MinimumSize.Height, config.BackdropHeight);
        Bounds = new Rectangle(config.BackdropX, config.BackdropY, width, height);
    }

    public void ApplyLocked(bool locked)
    {
        if (_locked == locked && FormBorderStyle == (locked ? FormBorderStyle.None : FormBorderStyle.SizableToolWindow))
            return;

        _locked = locked;
        FormBorderStyle = locked ? FormBorderStyle.None : FormBorderStyle.SizableToolWindow;
        Cursor = locked ? Cursors.Default : Cursors.SizeAll;
        LockedChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveBoundsToConfig(Config config)
    {
        config.BackdropX = Bounds.X;
        config.BackdropY = Bounds.Y;
        config.BackdropWidth = Math.Max(MinimumSize.Width, Bounds.Width);
        config.BackdropHeight = Math.Max(MinimumSize.Height, Bounds.Height);
        config.BackdropLocked = _locked;
    }

    private void DragWindowMouseDown(object? sender, MouseEventArgs e)
    {
        if (_locked || e.Button != MouseButtons.Left) return;
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
    }
}
