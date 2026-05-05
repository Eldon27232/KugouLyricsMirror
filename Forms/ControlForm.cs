namespace KugouLyricsMirror;

internal sealed class ControlForm : Form
{
    private readonly ComboBox _captureModeSelector = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly NumericUpDown _x = new() { Maximum = 100000, Minimum = -100000, Width = 90 };
    private readonly NumericUpDown _y = new() { Maximum = 100000, Minimum = -100000, Width = 90 };
    private readonly NumericUpDown _w = new() { Maximum = 100000, Minimum = 20, Value = 600, Width = 90 };
    private readonly NumericUpDown _h = new() { Maximum = 100000, Minimum = 20, Value = 120, Width = 90 };
    private readonly NumericUpDown _fps = new() { Maximum = 60, Minimum = 1, Value = 20, Width = 90 };
    private readonly NumericUpDown _threshold = new() { Maximum = 255, Minimum = 0, Value = 36, Width = 90 };
    private readonly CheckBox _topMost = new() { Text = "预览窗置顶" };
    private readonly CheckBox _excludeFromCapture = new() { Text = "预览窗不参与屏幕捕获（开了会导致 SteamVR 抓黑）", Checked = false, AutoSize = true };
    private readonly CheckBox _showBackdrop = new() { Text = "显示黑色底板", AutoSize = true };
    private readonly CheckBox _lockBackdrop = new() { Text = "锁定底板", AutoSize = true };
    private readonly Label _colorMeaning = new() { Text = "当前抠色", AutoSize = true, Margin = new Padding(8, 7, 0, 0) };
    private readonly CheckBox _autoRegionKeyColor = new() { Text = "自动", Checked = true, AutoSize = true, Margin = new Padding(8, 5, 3, 3) };
    private readonly NumericUpDown _colorR = new() { Maximum = 255, Minimum = 0, Width = 56 };
    private readonly NumericUpDown _colorG = new() { Maximum = 255, Minimum = 0, Width = 56 };
    private readonly NumericUpDown _colorB = new() { Maximum = 255, Minimum = 0, Width = 56 };
    private readonly TextBox _hexColor = new() { Width = 82, CharacterCasing = CharacterCasing.Upper };
    private readonly Button _resetColor = new() { Text = "恢复默认", AutoSize = true };
    private readonly Panel _colorPreview = new() { Width = 48, Height = 24, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(8, 3, 3, 3) };
    private readonly Label _status = new() { AutoSize = true, Text = "未启动" };
    private bool _syncingColorUi;

    private PreviewForm? _preview;
    private BackdropForm? _backdrop;
    private readonly Button _start = new() { Text = "启动镜像", AutoSize = true };
    private readonly Button _stop = new() { Text = "停止", AutoSize = true, Enabled = false };
    private readonly Button _pickRegion = new() { Text = "框选区域", AutoSize = true };
    private readonly Button _pickColor = new() { Text = "吸管取色", AutoSize = true };
    private readonly Button _apply = new() { Text = "应用到预览窗", AutoSize = true };
    private readonly Button _alignBackdrop = new() { Text = "底板对齐源区域", AutoSize = true };
    private readonly ListBox _windowList = new() { Width = 760, Height = 140, HorizontalScrollbar = true };
    private readonly Button _scanWindows = new() { Text = "扫描窗口", AutoSize = true };
    private readonly Button _bindWindow = new() { Text = "绑定窗口", AutoSize = true };
    private readonly Button _refreshDwm = new() { Text = "刷新代理", AutoSize = true };
    private readonly Label _selectedWindow = new() { AutoSize = true, Text = "未绑定窗口" };

    public ControlForm()
    {
        Text = "KugouLyricsMirror";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _captureModeSelector.Items.AddRange(["窗口代理 DWM", "窗口捕获抠色", "区域抠色"]);
        LoadConfigToUi();

        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 13,
            AutoSize = true,
            Padding = new Padding(12),
            Dock = DockStyle.Fill
        };

        void AddRow(string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 8) });
            grid.Controls.Add(control);
        }

        AddRow("捕获模式", _captureModeSelector);

        var windowButtons = new FlowLayoutPanel { AutoSize = true };
        windowButtons.Controls.AddRange([_scanWindows, _bindWindow, _refreshDwm]);
        AddRow("窗口操作", windowButtons);
        AddRow("窗口列表", _windowList);
        AddRow("当前窗口", _selectedWindow);

        AddRow("源区域 X", _x);
        AddRow("源区域 Y", _y);
        AddRow("宽度", _w);
        AddRow("高度", _h);
        AddRow("帧率", _fps);

        var keyColorRow = new FlowLayoutPanel { AutoSize = true };
        keyColorRow.Controls.Add(_pickColor);
        keyColorRow.Controls.Add(_autoRegionKeyColor);
        keyColorRow.Controls.Add(_colorMeaning);
        keyColorRow.Controls.Add(_colorPreview);
        keyColorRow.Controls.Add(new Label { Text = "R", AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
        keyColorRow.Controls.Add(_colorR);
        keyColorRow.Controls.Add(new Label { Text = "G", AutoSize = true, Margin = new Padding(6, 7, 0, 0) });
        keyColorRow.Controls.Add(_colorG);
        keyColorRow.Controls.Add(new Label { Text = "B", AutoSize = true, Margin = new Padding(6, 7, 0, 0) });
        keyColorRow.Controls.Add(_colorB);
        keyColorRow.Controls.Add(new Label { Text = "#", AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
        keyColorRow.Controls.Add(_hexColor);
        keyColorRow.Controls.Add(_resetColor);
        AddRow("背景色", keyColorRow);

        AddRow("阈值", _threshold);
        AddRow("选项", new FlowLayoutPanel { AutoSize = true, Controls = { _topMost, _excludeFromCapture } });
        AddRow("黑色底板", new FlowLayoutPanel { AutoSize = true, Controls = { _showBackdrop, _lockBackdrop, _alignBackdrop } });

        var buttonRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttonRow.Controls.AddRange([_pickRegion, _apply, _start, _stop]);
        AddRow("操作", buttonRow);
        AddRow("状态", _status);

        Controls.Add(grid);

        _pickRegion.Click += (_, _) => PickRegion();
        _pickColor.Click += (_, _) => PickColor();
        _apply.Click += (_, _) => ApplyToPreview();
        _start.Click += (_, _) => StartPreview();
        _stop.Click += (_, _) => StopPreview();
        _captureModeSelector.SelectedIndexChanged += (_, _) => CaptureModeChanged();
        _scanWindows.Click += (_, _) => ScanWindows();
        _bindWindow.Click += (_, _) => BindSelectedWindow();
        _refreshDwm.Click += (_, _) => RefreshDwmProxy();
        _windowList.DoubleClick += (_, _) => BindSelectedWindow();
        _colorR.ValueChanged += (_, _) => ApplyRgbInputs();
        _colorG.ValueChanged += (_, _) => ApplyRgbInputs();
        _colorB.ValueChanged += (_, _) => ApplyRgbInputs();
        _hexColor.Leave += (_, _) => ApplyHexInput();
        _hexColor.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            ApplyHexInput();
            e.SuppressKeyPress = true;
        };
        _resetColor.Click += (_, _) => ResetKeyColorToDefault();
        _autoRegionKeyColor.CheckedChanged += (_, _) => RegionAutoKeyColorChanged();
        _showBackdrop.CheckedChanged += (_, _) => ToggleBackdrop();
        _lockBackdrop.CheckedChanged += (_, _) => ApplyBackdropLock();
        _alignBackdrop.Click += (_, _) => AlignBackdropToSourceRegion();

        if (_showBackdrop.Checked)
            BeginInvoke((MethodInvoker)ToggleBackdrop);

        FormClosing += (_, _) =>
        {
            SaveUiToConfig();
            _preview?.Close();
            _preview?.Dispose();
            _backdrop?.Close();
            _backdrop?.Dispose();
        };
    }

    private void LoadConfigToUi()
    {
        _x.Value = AppConfig.Current.X;
        _y.Value = AppConfig.Current.Y;
        _w.Value = Math.Max(20, AppConfig.Current.Width);
        _h.Value = Math.Max(20, AppConfig.Current.Height);
        _fps.Value = Math.Clamp(AppConfig.Current.Fps, 1, 60);
        _threshold.Value = Math.Clamp(AppConfig.Current.ColorThreshold, 0, 255);
        _captureModeSelector.SelectedIndex = AppConfig.Current.CaptureMode switch
        {
            CaptureMode.WindowChromaKey => 1,
            CaptureMode.RegionChromaKey => 2,
            _ => 0
        };
        _topMost.Checked = AppConfig.Current.TopMost;
        _excludeFromCapture.Checked = AppConfig.Current.ExcludeFromCapture;
        _showBackdrop.Checked = AppConfig.Current.BackdropVisible;
        _lockBackdrop.Checked = AppConfig.Current.BackdropLocked;
        _autoRegionKeyColor.Checked = AppConfig.Current.RegionAutoKeyColor;
        SetColorInput(GetModeColor(), applyToPreview: false);
        UpdateSelectedWindowLabel();
        UpdateModeUi();
    }

    private bool SaveUiToConfig()
    {
        TryUpdateAutoRegionKeyColorFromScreen();
        _preview?.SavePreviewBoundsToConfig(AppConfig.Current);
        _backdrop?.SaveBoundsToConfig(AppConfig.Current);
        AppConfig.Current = new Config
        {
            X = (int)_x.Value,
            Y = (int)_y.Value,
            Width = (int)_w.Value,
            Height = (int)_h.Value,
            Fps = (int)_fps.Value,
            ColorThreshold = (int)_threshold.Value,
            KeyColorArgb = AppConfig.Current.KeyColorArgb,
            RegionKeyColorArgb = GetSelectedCaptureMode() == CaptureMode.RegionChromaKey
                ? _colorPreview.BackColor.ToArgb()
                : AppConfig.Current.RegionKeyColor.ToArgb(),
            RegionAutoKeyColor = _autoRegionKeyColor.Checked,
            WindowChromaFillColorArgb = GetSelectedCaptureMode() == CaptureMode.WindowChromaKey
                ? _colorPreview.BackColor.ToArgb()
                : AppConfig.Current.WindowChromaFillColor.ToArgb(),
            TopMost = _topMost.Checked,
            ExcludeFromCapture = _excludeFromCapture.Checked,
            CaptureMode = GetSelectedCaptureMode(),
            SourceWindowHandle = AppConfig.Current.SourceWindowHandle,
            BackdropVisible = _showBackdrop.Checked,
            BackdropLocked = _lockBackdrop.Checked,
            BackdropX = AppConfig.Current.BackdropX,
            BackdropY = AppConfig.Current.BackdropY,
            BackdropWidth = AppConfig.Current.BackdropWidth,
            BackdropHeight = AppConfig.Current.BackdropHeight,
            PreviewX = AppConfig.Current.PreviewX,
            PreviewY = AppConfig.Current.PreviewY
        };
        _preview?.SavePreviewBoundsToConfig(AppConfig.Current);
        _backdrop?.SaveBoundsToConfig(AppConfig.Current);
        if (AppConfig.Save(out var errorMessage))
            return true;

        _status.Text = $"配置保存失败: {errorMessage}";
        return false;
    }

    private void PickRegion()
    {
        using var picker = new RegionPickerForm();
        if (picker.ShowDialog(this) == DialogResult.OK)
        {
            var r = picker.SelectedRegion;
            _x.Value = r.X;
            _y.Value = r.Y;
            _w.Value = Math.Max(20, r.Width);
            _h.Value = Math.Max(20, r.Height);
            _status.Text = $"已选择区域: {r.X},{r.Y} {r.Width}x{r.Height}";
            ApplyToPreview();
        }
    }

    private void PickColor()
    {
        Hide();
        using var picker = new ColorPickerForm();
        if (picker.ShowDialog() == DialogResult.OK)
        {
            if (GetSelectedCaptureMode() == CaptureMode.RegionChromaKey)
                _autoRegionKeyColor.Checked = false;
            SetColorInput(picker.SelectedColor, applyToPreview: false);
            _status.Text = $"已取色: RGB({picker.SelectedColor.R},{picker.SelectedColor.G},{picker.SelectedColor.B})";
            ApplyToPreview();
        }
        Show();
        Activate();
    }

    private void ApplyToPreview()
    {
        if (!SaveUiToConfig()) return;
        if (_preview is not null && !_preview.ApplyConfig(AppConfig.Current, out var errorMessage))
        {
            _status.Text = $"应用失败: {errorMessage}";
            return;
        }
        ApplyToBackdrop();
        _status.Text = AppConfig.Current.CaptureMode switch
        {
            CaptureMode.DwmWindow => $"窗口代理已应用: 0x{AppConfig.Current.SourceWindowHandle:X}",
            CaptureMode.WindowChromaKey => $"窗口捕获抠色已应用: 0x{AppConfig.Current.SourceWindowHandle:X} / 黑底阈值 {AppConfig.Current.ColorThreshold} / 镜像背景色 RGB({AppConfig.Current.WindowChromaFillColor.R},{AppConfig.Current.WindowChromaFillColor.G},{AppConfig.Current.WindowChromaFillColor.B})",
            _ => GetRegionColorStatus()
        };
    }

    private void StartPreview()
    {
        if (!SaveUiToConfig()) return;

        _preview ??= new PreviewForm();
        _preview.StatusMessage -= PreviewStatusMessage;
        _preview.StatusMessage += PreviewStatusMessage;
        _preview.RegionAutoKeyColorChanged -= PreviewRegionAutoKeyColorChanged;
        _preview.RegionAutoKeyColorChanged += PreviewRegionAutoKeyColorChanged;
        if (!_preview.ApplyConfig(AppConfig.Current, out var errorMessage))
        {
            _status.Text = $"启动失败: {errorMessage}";
            return;
        }
        _preview.Show();
        _preview.BringToFront();

        _start.Enabled = false;
        _stop.Enabled = true;
        var runningStatus = AppConfig.Current.CaptureMode switch
        {
            CaptureMode.DwmWindow => "窗口代理运行中。把 Lyrics Mirror Preview 交给 SteamVR / OVR Toolkit / Desktop+ 去抓即可。",
            CaptureMode.WindowChromaKey => "窗口捕获抠色运行中。预览窗为普通可拖动窗口。",
            _ => "区域抠色运行中。把预览窗交给 SteamVR / OVR Toolkit / Desktop+ 去抓即可。"
        };

        if (!_preview.TryGetPreviewStyleStatus(out var styleStatus))
            _status.Text = styleStatus;
        else
        {
            var excludeWarning = AppConfig.Current.CaptureMode == CaptureMode.RegionChromaKey && AppConfig.Current.ExcludeFromCapture
                ? " ExcludeFromCapture 会导致 SteamVR 抓黑或抓不到，不建议开启。"
                : "";
            _status.Text = $"{runningStatus} {styleStatus}{excludeWarning}";
        }
    }

    private void StopPreview()
    {
        _preview?.Hide();
        _start.Enabled = true;
        _stop.Enabled = false;
        _status.Text = "已停止";
    }

    private void ToggleBackdrop()
    {
        if (!SaveUiToConfig()) return;

        if (_showBackdrop.Checked)
        {
            _backdrop ??= new BackdropForm();
            _backdrop.LockedChanged -= BackdropLockedChanged;
            _backdrop.LockedChanged += BackdropLockedChanged;
            _backdrop.ApplyConfig(AppConfig.Current);
            _backdrop.Show();
            _backdrop.SendToBack();
            _status.Text = "黑色底板已显示。把桌面歌词放到底板上方，再框选同一区域。";
        }
        else
        {
            _backdrop?.Hide();
            _status.Text = "黑色底板已隐藏";
        }

        UpdateModeUi();
    }

    private void ApplyBackdropLock()
    {
        AppConfig.Current.BackdropLocked = _lockBackdrop.Checked;
        _backdrop?.ApplyLocked(_lockBackdrop.Checked);
        _ = SaveUiToConfig();
    }

    private void AlignBackdropToSourceRegion()
    {
        AppConfig.Current.BackdropX = (int)_x.Value;
        AppConfig.Current.BackdropY = (int)_y.Value;
        AppConfig.Current.BackdropWidth = (int)_w.Value;
        AppConfig.Current.BackdropHeight = (int)_h.Value;

        if (!_showBackdrop.Checked)
            _showBackdrop.Checked = true;
        else
            ApplyToBackdrop();

        if (!SaveUiToConfig()) return;
        _status.Text = "黑色底板已对齐到当前源区域";
    }

    private void ApplyToBackdrop()
    {
        if (!_showBackdrop.Checked || _backdrop is null) return;
        _backdrop.ApplyConfig(AppConfig.Current);
        _backdrop.Show();
        _backdrop.SendToBack();
    }

    private void BackdropLockedChanged(object? sender, EventArgs e)
    {
        if (_backdrop is null) return;

        var locked = _backdrop.IsLocked;
        if (_lockBackdrop.Checked != locked)
            _lockBackdrop.Checked = locked;
    }

    private void CaptureModeChanged()
    {
        if (GetSelectedCaptureMode() == CaptureMode.WindowChromaKey)
        {
            SetColorInput(AppConfig.Current.WindowChromaFillColor, applyToPreview: false);
            if (_threshold.Value == 36)
                _threshold.Value = 16;
        }
        else
        {
            SetColorInput(GetModeColor(), applyToPreview: false);
        }

        UpdateModeUi();
        _ = SaveUiToConfig();
    }

    private string GetSelectedCaptureMode()
    {
        return _captureModeSelector.SelectedIndex switch
        {
            1 => CaptureMode.WindowChromaKey,
            2 => CaptureMode.RegionChromaKey,
            _ => CaptureMode.DwmWindow
        };
    }

    private void UpdateModeUi()
    {
        var mode = GetSelectedCaptureMode();
        var isDwm = mode == CaptureMode.DwmWindow;
        var isWindowCapture = mode == CaptureMode.WindowChromaKey;
        var isWindowMode = isDwm || isWindowCapture;
        var isRegion = mode == CaptureMode.RegionChromaKey;

        _windowList.Enabled = isWindowMode;
        _scanWindows.Enabled = isWindowMode;
        _bindWindow.Enabled = isWindowMode;
        _refreshDwm.Enabled = isDwm;

        _x.Enabled = isRegion;
        _y.Enabled = isRegion;
        _w.Enabled = isRegion;
        _h.Enabled = isRegion;
        _threshold.Enabled = isRegion || isWindowCapture;
        _pickRegion.Enabled = isRegion;
        _pickColor.Enabled = isRegion || isWindowCapture;
        _autoRegionKeyColor.Visible = isRegion;
        _colorMeaning.Text = isWindowCapture ? "镜像背景色" : "当前抠色";
        _colorR.Enabled = isRegion || isWindowCapture;
        _colorG.Enabled = isRegion || isWindowCapture;
        _colorB.Enabled = isRegion || isWindowCapture;
        _hexColor.Enabled = isRegion || isWindowCapture;
        _resetColor.Enabled = isRegion || isWindowCapture;
        _alignBackdrop.Enabled = isRegion;
        _showBackdrop.Enabled = isRegion;
        _lockBackdrop.Enabled = isRegion && _showBackdrop.Checked;
    }

    private void ScanWindows()
    {
        var windows = WindowEnumerator.EnumerateTopLevelWindows();
        _windowList.BeginUpdate();
        try
        {
            _windowList.Items.Clear();
            foreach (var window in windows)
                _windowList.Items.Add(window);
        }
        finally
        {
            _windowList.EndUpdate();
        }

        _status.Text = $"已扫描 {windows.Count} 个顶层窗口。工具窗口未过滤。";
    }

    private void BindSelectedWindow()
    {
        if (_windowList.SelectedItem is not WindowInfo window)
        {
            _status.Text = "请先在窗口列表中选择一个窗口";
            return;
        }

        AppConfig.Current.SourceWindowHandle = window.Hwnd.ToInt64();
        UpdateSelectedWindowLabel(window);
        if (!SaveUiToConfig()) return;

        if (_preview is not null && !_preview.ApplyConfig(AppConfig.Current, out var errorMessage))
            _status.Text = $"绑定失败: {errorMessage}。可切回区域抠色。";
        else
            _status.Text = $"已绑定窗口: {window.Title} | {window.ClassName} | 0x{window.Hwnd.ToInt64():X} | {window.Bounds.X},{window.Bounds.Y} {window.Bounds.Width}x{window.Bounds.Height}";
    }

    private void RefreshDwmProxy()
    {
        if (_preview is null)
        {
            _status.Text = "预览窗未启动";
            return;
        }

        if (_preview.RefreshThumbnail(out var errorMessage))
            _status.Text = "窗口代理已刷新";
        else
            _status.Text = $"窗口代理刷新失败: {errorMessage}。可切回区域抠色。";
    }

    private void UpdateSelectedWindowLabel(WindowInfo? window = null)
    {
        if (window is not null)
        {
            _selectedWindow.Text = $"{window.Title} | {window.ClassName} | 0x{window.Hwnd.ToInt64():X}";
            return;
        }

        _selectedWindow.Text = AppConfig.Current.SourceWindowHandle == 0
            ? "未绑定窗口"
            : $"已绑定 HWND: 0x{AppConfig.Current.SourceWindowHandle:X}";
    }

    private void PreviewStatusMessage(object? sender, string message)
    {
        _status.Text = message;
        if (message.Contains("源窗口", StringComparison.CurrentCulture))
        {
            _start.Enabled = true;
            _stop.Enabled = false;
        }
    }

    private void PreviewRegionAutoKeyColorChanged(object? sender, Color color)
    {
        if (GetSelectedCaptureMode() != CaptureMode.RegionChromaKey || !_autoRegionKeyColor.Checked)
            return;

        SetColorInput(color, applyToPreview: false);
    }

    private void TryUpdateAutoRegionKeyColorFromScreen()
    {
        if (GetSelectedCaptureMode() != CaptureMode.RegionChromaKey || !_autoRegionKeyColor.Checked)
            return;

        var sourceRect = new Rectangle((int)_x.Value, (int)_y.Value, Math.Max(1, (int)_w.Value), Math.Max(1, (int)_h.Value));
        using var sample = new Bitmap(sourceRect.Width, sourceRect.Height);
        ScreenCapture.CopyScreenAreaToBitmap(sample, sourceRect);
        var color = ScreenCapture.EstimateBackgroundColor(sample);
        SetColorInput(color, applyToPreview: false);
        _status.Text = $"区域抠色：自动背景色 RGB({color.R},{color.G},{color.B}) / 阈值 {(int)_threshold.Value}";
    }

    private void SetColorInput(Color color, bool applyToPreview)
    {
        _syncingColorUi = true;
        try
        {
            _colorPreview.BackColor = color;
            _colorR.Value = color.R;
            _colorG.Value = color.G;
            _colorB.Value = color.B;
            _hexColor.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        finally
        {
            _syncingColorUi = false;
        }

        if (GetSelectedCaptureMode() == CaptureMode.WindowChromaKey)
            AppConfig.Current.WindowChromaFillColor = color;
        else
            AppConfig.Current.RegionKeyColor = color;

        if (applyToPreview)
        {
            _status.Text = GetSelectedCaptureMode() == CaptureMode.WindowChromaKey
                ? $"镜像背景色: RGB({color.R},{color.G},{color.B}) / #{color.R:X2}{color.G:X2}{color.B:X2}"
                : $"区域抠色：手动背景色 RGB({color.R},{color.G},{color.B}) / 阈值 {(int)_threshold.Value}";
            ApplyToPreview();
        }
    }

    private void ResetKeyColorToDefault()
    {
        if (GetSelectedCaptureMode() == CaptureMode.WindowChromaKey)
        {
            SetColorInput(Color.Lime, applyToPreview: false);
            _threshold.Value = 16;
            _status.Text = "已恢复窗口捕获抠色默认值：黑底抠除阈值 16，镜像背景使用透明兼容色。";
        }
        else
        {
            SetColorInput(Color.Black, applyToPreview: false);
            _status.Text = $"区域抠色：手动背景色 RGB(0,0,0) / 阈值 {(int)_threshold.Value}";
        }

        ApplyToPreview();
    }

    private void RegionAutoKeyColorChanged()
    {
        AppConfig.Current.RegionAutoKeyColor = _autoRegionKeyColor.Checked;
        if (_autoRegionKeyColor.Checked && GetSelectedCaptureMode() == CaptureMode.RegionChromaKey)
            _status.Text = "区域抠色：自动背景色已开启";
        else if (GetSelectedCaptureMode() == CaptureMode.RegionChromaKey)
            _status.Text = $"区域抠色：手动背景色 RGB({_colorPreview.BackColor.R},{_colorPreview.BackColor.G},{_colorPreview.BackColor.B}) / 阈值 {(int)_threshold.Value}";

        ApplyToPreview();
    }

    private void ApplyRgbInputs()
    {
        if (_syncingColorUi) return;
        if (GetSelectedCaptureMode() == CaptureMode.RegionChromaKey)
            _autoRegionKeyColor.Checked = false;
        SetColorInput(Color.FromArgb((int)_colorR.Value, (int)_colorG.Value, (int)_colorB.Value), applyToPreview: true);
    }

    private void ApplyHexInput()
    {
        if (_syncingColorUi) return;

        var text = _hexColor.Text.Trim();
        if (text.StartsWith("#", StringComparison.Ordinal))
            text = text[1..];

        if (text.Length != 6 || !int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out var value))
        {
            _status.Text = "十六进制颜色格式应为 RRGGBB，例如 000000 或 FFFFFF";
            SetColorInput(_colorPreview.BackColor, applyToPreview: false);
            return;
        }

        if (GetSelectedCaptureMode() == CaptureMode.RegionChromaKey)
            _autoRegionKeyColor.Checked = false;
        SetColorInput(Color.FromArgb((value >> 16) & 0xFF, (value >> 8) & 0xFF, value & 0xFF), applyToPreview: true);
    }

    private Color GetModeColor()
    {
        return GetSelectedCaptureMode() == CaptureMode.WindowChromaKey
            ? AppConfig.Current.WindowChromaFillColor
            : AppConfig.Current.RegionKeyColor;
    }

    private string GetRegionColorStatus()
    {
        var color = AppConfig.Current.RegionKeyColor;
        var source = AppConfig.Current.RegionAutoKeyColor ? "自动" : "手动";
        return $"区域抠色：{source}背景色 RGB({color.R},{color.G},{color.B}) / 阈值 {AppConfig.Current.ColorThreshold}";
    }
}
