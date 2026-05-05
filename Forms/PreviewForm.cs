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
    private Bitmap? _renderBuffer;
    private Rectangle _sourceRect;
    private string _captureMode = CaptureMode.DwmWindow;
    private IntPtr _sourceWindow;
    private IntPtr _thumbnail;
    private bool _clickThrough;
    private bool _useLayeredTransparency;
    private bool _excludeFromCaptureRequested;
    private bool _previewLocationInitialized;
    private bool _followSourceWindow;
    private int _fps = 20;
    private int _threshold = 36;
    private bool _regionAutoKeyColor = true;
    private Color _regionKeyColor = Color.Black;
    private Color _windowChromaFillColor = TransparentColor;
    private Color? _lastReportedAutoKeyColor;

    public event EventHandler<string>? StatusMessage;
    public event EventHandler<Color>? RegionAutoKeyColorChanged;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle &= ~NativeMethods.WS_EX_TOOLWINDOW;
            cp.ExStyle |= NativeMethods.WS_EX_APPWINDOW;

            if (_useLayeredTransparency)
                cp.ExStyle |= NativeMethods.WS_EX_LAYERED;

            if (_clickThrough)
            {
                cp.ExStyle |= NativeMethods.WS_EX_TRANSPARENT;
            }

            return cp;
        }
    }

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
            _ = ApplyConfig(AppConfig.Current, out _);
        };

        MouseDown += DragWindowMouseDown;
        _picture.MouseDown += DragWindowMouseDown;

        var menu = new ContextMenuStrip();
        menu.Items.Add("隐藏", null, (_, _) => Hide());
        menu.Items.Add("退出", null, (_, _) => Close());
        ContextMenuStrip = menu;
        _picture.ContextMenuStrip = menu;
    }

    public bool ApplyConfig(Config config, out string? errorMessage)
    {
        errorMessage = null;
        _captureMode = config.CaptureMode switch
        {
            CaptureMode.RegionChromaKey => CaptureMode.RegionChromaKey,
            CaptureMode.WindowChromaKey => CaptureMode.WindowChromaKey,
            _ => CaptureMode.DwmWindow
        };
        _sourceRect = new Rectangle(config.X, config.Y, Math.Max(1, config.Width), Math.Max(1, config.Height));
        _sourceWindow = config.SourceWindowHandle == 0 ? IntPtr.Zero : new IntPtr(config.SourceWindowHandle);
        _fps = Math.Clamp(config.Fps, 1, 60);
        _threshold = Math.Clamp(config.ColorThreshold, 0, 255);
        _regionAutoKeyColor = config.RegionAutoKeyColor;
        _regionKeyColor = config.RegionKeyColor;
        _windowChromaFillColor = config.WindowChromaFillColor;
        _followSourceWindow = config.WindowFollowSourceWindow;
        _clickThrough = _captureMode == CaptureMode.WindowChromaKey && config.WindowClickThrough;
        TopMost = config.TopMost;
        _excludeFromCaptureRequested = config.ExcludeFromCapture;
        TryExcludeFromCapture(_captureMode == CaptureMode.DwmWindow && config.ExcludeFromCapture);

        if (_captureMode == CaptureMode.DwmWindow)
            return ApplyDwmMode(out errorMessage);

        if (_captureMode == CaptureMode.WindowChromaKey)
            return ApplyWindowChromaKeyMode(out errorMessage);

        ApplyRegionMode();
        return true;
    }

    public bool RefreshThumbnail(out string? errorMessage)
    {
        errorMessage = null;
        if (_captureMode != CaptureMode.DwmWindow)
            return true;

        UnregisterThumbnail();
        return RegisterThumbnail(out errorMessage);
    }

    public bool TryGetPreviewStyleStatus(out string message)
    {
        if (!IsHandleCreated)
        {
            message = "Preview ex=未创建";
            return true;
        }

        var exStyle = NativeMethods.GetWindowLongPtr(Handle, NativeMethods.GWL_EXSTYLE).ToInt64();
        if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0)
        {
            message = _captureMode == CaptureMode.RegionChromaKey
                ? "错误：区域抠色 Preview 仍是 WS_EX_TOOLWINDOW，SteamVR 可能抓不到。"
                : "错误：Preview 窗口仍是 WS_EX_TOOLWINDOW，SteamVR 可能抓不到。";
            return false;
        }

        if ((exStyle & NativeMethods.WS_EX_APPWINDOW) == 0)
        {
            message = $"错误：Preview 窗口缺少 WS_EX_APPWINDOW ex=0x{exStyle:X}";
            return false;
        }

        message = $"Preview mode={_captureMode}";
        if (_captureMode is CaptureMode.RegionChromaKey or CaptureMode.WindowChromaKey)
        {
            var hasTransparent = (exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0;
            var hasNoActivate = (exStyle & NativeMethods.WS_EX_NOACTIVATE) != 0;
            var expectedTransparent = _captureMode == CaptureMode.WindowChromaKey && _clickThrough;
            if (hasNoActivate || hasTransparent != expectedTransparent)
            {
                message = $"错误：{_captureMode} Preview 样式异常 ex=0x{exStyle:X} transparent={hasTransparent} noActivate={hasNoActivate}";
                return false;
            }

            var follow = _captureMode == CaptureMode.WindowChromaKey && _followSourceWindow;
            var clickThrough = _captureMode == CaptureMode.WindowChromaKey && _clickThrough;
            message = $"{message} follow={follow.ToString().ToLowerInvariant()} clickThrough={clickThrough.ToString().ToLowerInvariant()} noActivate=false ex=0x{exStyle:X}";
        }
        else
        {
            message = $"{message} ex=0x{exStyle:X}";
        }
        return true;
    }

    public void SavePreviewBoundsToConfig(Config config)
    {
        if (!IsDisposed && Bounds.Width > 0 && Bounds.Height > 0)
        {
            config.PreviewX = Bounds.X;
            config.PreviewY = Bounds.Y;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_captureMode == CaptureMode.DwmWindow && _thumbnail != IntPtr.Zero)
            _ = UpdateThumbnailDestination(out _);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyPreviewWindowStyles(_useLayeredTransparency, _clickThrough);
    }

    private void ApplyRegionMode()
    {
        UnregisterThumbnail();
        DisposeFrameBuffers();
        ShowInTaskbar = true;
        _picture.Visible = true;
        BackColor = TransparentColor;
        TransparencyKey = TransparentColor;
        ApplyPreviewWindowStyles(enableLayeredTransparency: false, enableClickThrough: false);

        if (ClientSize.Width != _sourceRect.Width || ClientSize.Height != _sourceRect.Height)
            ClientSize = new Size(_sourceRect.Width, _sourceRect.Height);

        ApplyPreviewLocation(AppConfig.Current, _sourceRect);
        _timer.Interval = Math.Max(15, 1000 / _fps);

        if (Visible && !_timer.Enabled)
            _timer.Start();
        else if (!Visible)
            _timer.Stop();

        if (_excludeFromCaptureRequested)
            StatusMessage?.Invoke(this, "ExcludeFromCapture 会导致 SteamVR 抓黑或抓不到，不建议开启。");
    }

    private bool ApplyWindowChromaKeyMode(out string? errorMessage)
    {
        errorMessage = string.Empty;
        UnregisterThumbnail();
        DisposeFrameBuffers();
        _picture.Visible = true;
        BackColor = TransparentColor;
        TransparencyKey = TransparentColor;
        ShowInTaskbar = true;
        TopMost = true;
        _timer.Interval = Math.Max(15, 1000 / _fps);
        ApplyPreviewWindowStyles(enableLayeredTransparency: true, enableClickThrough: _clickThrough);

        if (_sourceWindow == IntPtr.Zero || !NativeMethods.IsWindow(_sourceWindow))
        {
            errorMessage = "请先绑定有效源窗口";
            return false;
        }

        if (!WindowCapture.TryGetWindowBounds(_sourceWindow, out var bounds, out errorMessage))
            return false;

        if (_followSourceWindow)
        {
            if (!UpdateOverlayBounds(out errorMessage))
                return false;
        }
        else if (ClientSize.Width != bounds.Width || ClientSize.Height != bounds.Height)
        {
            ClientSize = new Size(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height));
        }

        if (!_followSourceWindow)
            ApplyPreviewLocation(AppConfig.Current, bounds);

        if (Visible && !_timer.Enabled)
            _timer.Start();
        else if (!Visible)
            _timer.Stop();

        return true;
    }

    private bool ApplyDwmMode(out string? errorMessage)
    {
        errorMessage = string.Empty;
        _timer.Stop();
        DisposeFrameBuffers();
        ShowInTaskbar = true;
        _picture.Visible = false;
        BackColor = Color.Black;
        TransparencyKey = Color.Empty;
        ApplyPreviewWindowStyles(enableLayeredTransparency: false, enableClickThrough: false);

        if (!Visible)
            return true;

        return RegisterThumbnail(out errorMessage);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_captureMode == CaptureMode.DwmWindow)
            _ = RegisterThumbnail(out _);
        else if (_captureMode == CaptureMode.WindowChromaKey)
            _timer.Start();
        else
            _timer.Start();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (_captureMode == CaptureMode.DwmWindow)
        {
            if (Visible)
                _ = RegisterThumbnail(out _);
            else
                UnregisterThumbnail();
        }
        else
        {
            _timer.Enabled = Visible;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        UnregisterThumbnail();
        DisposeFrameBuffers();
        base.OnFormClosed(e);
    }

    private void CaptureFrame()
    {
        if (_captureMode == CaptureMode.WindowChromaKey)
        {
            CaptureWindowFrame();
            return;
        }

        if (_sourceRect.Width <= 0 || _sourceRect.Height <= 0) return;

        EnsureRenderBuffer();
        if (_renderBuffer is null) return;

        ScreenCapture.CopyScreenAreaToBitmap(_renderBuffer, _sourceRect);
        if (_regionAutoKeyColor)
            UpdateAutoRegionKeyColor(_renderBuffer);

        ChromaKeyProcessor.Apply(_renderBuffer, _regionKeyColor, _threshold, TransparentColor);

        var previousDisplay = _picture.Image as Bitmap;
        _picture.Image = _renderBuffer;
        _renderBuffer = previousDisplay;
    }

    private void CaptureWindowFrame()
    {
        if (_sourceWindow == IntPtr.Zero || !NativeMethods.IsWindow(_sourceWindow))
        {
            _timer.Stop();
            StatusMessage?.Invoke(this, "源窗口已关闭或失效");
            return;
        }

        if (_followSourceWindow && !UpdateOverlayBounds(out var boundsError))
        {
            _timer.Stop();
            StatusMessage?.Invoke(this, boundsError ?? "源窗口已关闭或失效");
            return;
        }

        if (!WindowCapture.TryCapture(_sourceWindow, out var frame, out var bounds, out var captureError) || frame is null)
        {
            StatusMessage?.Invoke(this, captureError ?? "源窗口捕获失败");
            return;
        }

        if (!_followSourceWindow && (ClientSize.Width != bounds.Width || ClientSize.Height != bounds.Height))
            ClientSize = new Size(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height));

        ChromaKeyProcessor.Apply(frame, Color.Black, _threshold, _windowChromaFillColor);

        var old = _picture.Image;
        _picture.Image = frame;
        old?.Dispose();
    }

    private void EnsureRenderBuffer()
    {
        if (_renderBuffer is not null
            && _renderBuffer.Width == _sourceRect.Width
            && _renderBuffer.Height == _sourceRect.Height)
        {
            return;
        }

        if (_renderBuffer is null)
        {
            _renderBuffer = new Bitmap(_sourceRect.Width, _sourceRect.Height);
            return;
        }

        DisposeFrameBuffers();
        _renderBuffer = new Bitmap(_sourceRect.Width, _sourceRect.Height);
    }

    private void UpdateAutoRegionKeyColor(Bitmap frame)
    {
        var color = ScreenCapture.EstimateBackgroundColor(frame);
        _regionKeyColor = color;

        if (_lastReportedAutoKeyColor.HasValue && ColorsNear(_lastReportedAutoKeyColor.Value, color))
            return;

        _lastReportedAutoKeyColor = color;
        RegionAutoKeyColorChanged?.Invoke(this, color);
        StatusMessage?.Invoke(this, $"区域抠色：自动背景色 RGB({color.R},{color.G},{color.B}) / 阈值 {_threshold}");
    }

    private static bool ColorsNear(Color left, Color right)
    {
        return Math.Abs(left.R - right.R) <= 2
            && Math.Abs(left.G - right.G) <= 2
            && Math.Abs(left.B - right.B) <= 2;
    }

    private void DisposeFrameBuffers()
    {
        if (_picture.Image is Image img)
        {
            _picture.Image = null;
            img.Dispose();
        }

        _renderBuffer?.Dispose();
        _renderBuffer = null;
    }

    private void DragWindowMouseDown(object? sender, MouseEventArgs e)
    {
        if (_clickThrough) return;
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

    private bool RegisterThumbnail(out string? errorMessage)
    {
        errorMessage = string.Empty;
        if (!IsHandleCreated || _sourceWindow == IntPtr.Zero)
        {
            errorMessage = "未选择源窗口";
            return false;
        }

        UnregisterThumbnail();

        var hr = NativeMethods.DwmRegisterThumbnail(Handle, _sourceWindow, out _thumbnail);
        if (hr != 0)
        {
            _thumbnail = IntPtr.Zero;
            errorMessage = $"DwmRegisterThumbnail 失败: 0x{hr:X8}";
            return false;
        }

        hr = NativeMethods.DwmQueryThumbnailSourceSize(_thumbnail, out var sourceSize);
        if (hr == 0 && sourceSize.Width > 0 && sourceSize.Height > 0
            && (ClientSize.Width != sourceSize.Width || ClientSize.Height != sourceSize.Height))
        {
            ClientSize = new Size(sourceSize.Width, sourceSize.Height);
        }

        return UpdateThumbnailDestination(out errorMessage);
    }

    private bool UpdateThumbnailDestination(out string? errorMessage)
    {
        errorMessage = string.Empty;
        if (_thumbnail == IntPtr.Zero)
            return true;

        const uint DWM_TNP_RECTDESTINATION = 0x00000001;
        const uint DWM_TNP_OPACITY = 0x00000004;
        const uint DWM_TNP_VISIBLE = 0x00000008;
        const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;

        var props = new DWM_THUMBNAIL_PROPERTIES
        {
            dwFlags = DWM_TNP_RECTDESTINATION
                | DWM_TNP_OPACITY
                | DWM_TNP_VISIBLE
                | DWM_TNP_SOURCECLIENTAREAONLY,
            rcDestination = new RECT(0, 0, Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height)),
            opacity = 255,
            fVisible = true,
            fSourceClientAreaOnly = false
        };

        var hr = NativeMethods.DwmUpdateThumbnailProperties(_thumbnail, ref props);
        if (hr == 0)
            return true;

        errorMessage = $"DwmUpdateThumbnailProperties 失败: 0x{hr:X8}";
        return false;
    }

    private void UnregisterThumbnail()
    {
        if (_thumbnail == IntPtr.Zero) return;

        _ = NativeMethods.DwmUnregisterThumbnail(_thumbnail);
        _thumbnail = IntPtr.Zero;
    }

    private bool UpdateOverlayBounds(out string? errorMessage)
    {
        if (!WindowCapture.TryGetWindowBounds(_sourceWindow, out var bounds, out errorMessage))
            return false;

        if (Bounds != bounds)
        {
            _ = NativeMethods.SetWindowPos(
                Handle,
                TopMost ? NativeMethods.HWND_TOPMOST : NativeMethods.HWND_NOTOPMOST,
                bounds.X,
                bounds.Y,
                Math.Max(1, bounds.Width),
                Math.Max(1, bounds.Height),
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        if (ClientSize.Width != bounds.Width || ClientSize.Height != bounds.Height)
            ClientSize = new Size(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height));

        return true;
    }

    private void ApplyPreviewLocation(Config config, Rectangle referenceRect)
    {
        if (_previewLocationInitialized && Visible)
            return;

        if (config.PreviewX.HasValue && config.PreviewY.HasValue)
        {
            Location = new Point(config.PreviewX.Value, config.PreviewY.Value);
            _previewLocationInitialized = true;
            return;
        }

        var screen = Screen.FromRectangle(referenceRect).WorkingArea;
        Location = new Point(
            screen.Left + Math.Max(0, (screen.Width - Width) / 2),
            screen.Top + Math.Max(0, (screen.Height - Height) / 2));
        _previewLocationInitialized = true;
    }

    private void ApplyPreviewWindowStyles(bool enableLayeredTransparency, bool enableClickThrough)
    {
        _useLayeredTransparency = enableLayeredTransparency;
        _clickThrough = enableClickThrough;
        if (!IsHandleCreated)
            return;

        ApplyCommonPreviewWindowStyles();
    }

    private void ApplyCommonPreviewWindowStyles()
    {
        if (!IsHandleCreated)
            return;

        var exStyle = NativeMethods.GetWindowLongPtr(Handle, NativeMethods.GWL_EXSTYLE).ToInt64();
        long clickThroughFlags = NativeMethods.WS_EX_TRANSPARENT;

        exStyle &= ~NativeMethods.WS_EX_TOOLWINDOW;
        exStyle |= NativeMethods.WS_EX_APPWINDOW;
        exStyle = _useLayeredTransparency
            ? exStyle | NativeMethods.WS_EX_LAYERED
            : exStyle & ~NativeMethods.WS_EX_LAYERED;
        exStyle = _clickThrough ? exStyle | clickThroughFlags : exStyle & ~clickThroughFlags;

        _ = NativeMethods.SetWindowLongPtr(Handle, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));
        _ = NativeMethods.SetWindowPos(
            Handle,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            NativeMethods.SWP_NOMOVE
                | NativeMethods.SWP_NOSIZE
                | NativeMethods.SWP_NOZORDER
                | NativeMethods.SWP_NOACTIVATE
                | NativeMethods.SWP_FRAMECHANGED);

        ReportPreviewWindowStyle();
    }

    private void ReportPreviewWindowStyle()
    {
        if (!IsHandleCreated)
            return;

        var exStyle = NativeMethods.GetWindowLongPtr(Handle, NativeMethods.GWL_EXSTYLE).ToInt64();
        if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0)
        {
            var error = _captureMode == CaptureMode.RegionChromaKey
                ? "错误：区域抠色 Preview 仍是 WS_EX_TOOLWINDOW，SteamVR 可能抓不到。"
                : "错误：Preview 窗口仍是 WS_EX_TOOLWINDOW，SteamVR 可能抓不到。";
            StatusMessage?.Invoke(this, error);
            return;
        }

        if ((exStyle & NativeMethods.WS_EX_APPWINDOW) == 0)
        {
            StatusMessage?.Invoke(this, $"错误：Preview 窗口缺少 WS_EX_APPWINDOW ex=0x{exStyle:X}");
            return;
        }

        var message = $"Preview mode={_captureMode}";
        if (_captureMode is CaptureMode.RegionChromaKey or CaptureMode.WindowChromaKey)
        {
            var hasTransparent = (exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0;
            var hasNoActivate = (exStyle & NativeMethods.WS_EX_NOACTIVATE) != 0;
            var expectedTransparent = _captureMode == CaptureMode.WindowChromaKey && _clickThrough;
            if (hasNoActivate || hasTransparent != expectedTransparent)
            {
                StatusMessage?.Invoke(this, $"错误：{_captureMode} Preview 样式异常 ex=0x{exStyle:X} transparent={hasTransparent} noActivate={hasNoActivate}");
                return;
            }

            var follow = _captureMode == CaptureMode.WindowChromaKey && _followSourceWindow;
            var clickThrough = _captureMode == CaptureMode.WindowChromaKey && _clickThrough;
            message = $"{message} follow={follow.ToString().ToLowerInvariant()} clickThrough={clickThrough.ToString().ToLowerInvariant()} noActivate=false ex=0x{exStyle:X}";
        }
        else
        {
            message = $"{message} ex=0x{exStyle:X}";
        }

        StatusMessage?.Invoke(this, message);
    }
}
