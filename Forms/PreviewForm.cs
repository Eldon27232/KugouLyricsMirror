namespace KugouLyricsMirror;

internal sealed class PreviewForm : Form
{
    private static readonly Color TransparentColor = Color.Lime;

    private readonly PictureBox _picture = new()
    {
        Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.StretchImage,
        BackColor = Color.Lime
    };

    private readonly System.Windows.Forms.Timer _timer = new();
    private Bitmap? _buffer;
    private Rectangle _sourceRect;
    private int _fps = 20;
    private int _threshold = 36;
    private Color _keyColor = Color.Black;

    public PreviewForm()
    {
        Text = "Lyrics Mirror Preview";
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = true;
        TopLevel = true;
        BackColor = TransparentColor;
        TransparencyKey = TransparentColor;
        Controls.Add(_picture);
        DoubleBuffered = true;
        Width = 600;
        Height = 120;

        _timer.Tick += (_, _) => CaptureFrame();
        Shown += (_, _) =>
        {
            TryExcludeFromCapture(AppConfig.Current.ExcludeFromCapture);
            CaptureFrame();
        };

        MouseDown += DragWindowMouseDown;
        _picture.MouseDown += DragWindowMouseDown;

        var menu = new ContextMenuStrip();
        menu.Items.Add("隐藏", null, (_, _) => Hide());
        menu.Items.Add("退出", null, (_, _) => Close());
        ContextMenuStrip = menu;
        _picture.ContextMenuStrip = menu;
    }

    public void ApplyConfig(Config config)
    {
        _sourceRect = new Rectangle(config.X, config.Y, Math.Max(1, config.Width), Math.Max(1, config.Height));
        _fps = Math.Clamp(config.Fps, 1, 60);
        _threshold = Math.Clamp(config.ColorThreshold, 0, 255);
        _keyColor = config.KeyColor;
        TopMost = config.TopMost;

        if (ClientSize.Width != _sourceRect.Width || ClientSize.Height != _sourceRect.Height)
            ClientSize = new Size(_sourceRect.Width, _sourceRect.Height);

        _timer.Interval = Math.Max(15, 1000 / _fps);
        TryExcludeFromCapture(config.ExcludeFromCapture);

        if (Visible && !_timer.Enabled)
            _timer.Start();
        else if (!Visible)
            _timer.Stop();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _timer.Start();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        _timer.Enabled = Visible;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _buffer?.Dispose();
        if (_picture.Image is Image img) img.Dispose();
        base.OnFormClosed(e);
    }

    private void CaptureFrame()
    {
        if (_sourceRect.Width <= 0 || _sourceRect.Height <= 0) return;

        _buffer ??= new Bitmap(_sourceRect.Width, _sourceRect.Height);
        if (_buffer.Width != _sourceRect.Width || _buffer.Height != _sourceRect.Height)
        {
            _buffer.Dispose();
            _buffer = new Bitmap(_sourceRect.Width, _sourceRect.Height);
        }

        ScreenCapture.CopyScreenAreaToBitmap(_buffer, _sourceRect);

        var frame = (Bitmap)_buffer.Clone();
        ChromaKeyProcessor.Apply(frame, _keyColor, _threshold, TransparentColor);

        var old = _picture.Image;
        _picture.Image = frame;
        old?.Dispose();
    }

    private void DragWindowMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
    }

    private void TryExcludeFromCapture(bool enable)
    {
        try
        {
            if (!IsHandleCreated) return;
            const uint WDA_NONE = 0x00000000;
            const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
            _ = NativeMethods.SetWindowDisplayAffinity(Handle, enable ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE);
        }
        catch
        {
        }
    }
}
