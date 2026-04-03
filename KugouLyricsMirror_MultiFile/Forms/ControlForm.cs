namespace KugouLyricsMirror;

internal sealed class ControlForm : Form
{
    private readonly NumericUpDown _x = new() { Maximum = 100000, Minimum = -100000, Width = 90 };
    private readonly NumericUpDown _y = new() { Maximum = 100000, Minimum = -100000, Width = 90 };
    private readonly NumericUpDown _w = new() { Maximum = 100000, Minimum = 20, Value = 600, Width = 90 };
    private readonly NumericUpDown _h = new() { Maximum = 100000, Minimum = 20, Value = 120, Width = 90 };
    private readonly NumericUpDown _fps = new() { Maximum = 60, Minimum = 1, Value = 20, Width = 90 };
    private readonly NumericUpDown _threshold = new() { Maximum = 255, Minimum = 0, Value = 36, Width = 90 };
    private readonly CheckBox _topMost = new() { Text = "预览窗置顶" };
    private readonly CheckBox _excludeFromCapture = new() { Text = "预览窗不参与屏幕捕获（开了会导致 SteamVR 抓黑）", Checked = false, AutoSize = true };
    private readonly Panel _colorPreview = new() { Width = 48, Height = 24, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(8, 3, 3, 3) };
    private readonly Label _status = new() { AutoSize = true, Text = "未启动" };

    private PreviewForm? _preview;
    private readonly Button _start = new() { Text = "启动镜像", AutoSize = true };
    private readonly Button _stop = new() { Text = "停止", AutoSize = true, Enabled = false };
    private readonly Button _pickRegion = new() { Text = "框选区域", AutoSize = true };
    private readonly Button _pickColor = new() { Text = "吸管取色", AutoSize = true };
    private readonly Button _apply = new() { Text = "应用到预览窗", AutoSize = true };

    public ControlForm()
    {
        Text = "KugouLyricsMirror";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        LoadConfigToUi();

        var grid = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 9,
            AutoSize = true,
            Padding = new Padding(12),
            Dock = DockStyle.Fill
        };

        void AddRow(string label, Control control)
        {
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 8) });
            grid.Controls.Add(control);
        }

        AddRow("源区域 X", _x);
        AddRow("源区域 Y", _y);
        AddRow("宽度", _w);
        AddRow("高度", _h);
        AddRow("帧率", _fps);

        var keyColorRow = new FlowLayoutPanel { AutoSize = true };
        keyColorRow.Controls.Add(_pickColor);
        keyColorRow.Controls.Add(new Label { Text = "当前抠色", AutoSize = true, Margin = new Padding(8, 7, 0, 0) });
        keyColorRow.Controls.Add(_colorPreview);
        AddRow("背景色", keyColorRow);

        AddRow("阈值", _threshold);
        AddRow("选项", new FlowLayoutPanel { AutoSize = true, Controls = { _topMost, _excludeFromCapture } });

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

        FormClosing += (_, _) =>
        {
            SaveUiToConfig();
            _preview?.Close();
            _preview?.Dispose();
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
        _topMost.Checked = AppConfig.Current.TopMost;
        _excludeFromCapture.Checked = AppConfig.Current.ExcludeFromCapture;
        _colorPreview.BackColor = AppConfig.Current.KeyColor;
    }

    private void SaveUiToConfig()
    {
        AppConfig.Current = new Config
        {
            X = (int)_x.Value,
            Y = (int)_y.Value,
            Width = (int)_w.Value,
            Height = (int)_h.Value,
            Fps = (int)_fps.Value,
            ColorThreshold = (int)_threshold.Value,
            KeyColorArgb = _colorPreview.BackColor.ToArgb(),
            TopMost = _topMost.Checked,
            ExcludeFromCapture = _excludeFromCapture.Checked
        };
        AppConfig.Save();
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
            AppConfig.Current.KeyColor = picker.SelectedColor;
            _colorPreview.BackColor = picker.SelectedColor;
            _status.Text = $"已取色: RGB({picker.SelectedColor.R},{picker.SelectedColor.G},{picker.SelectedColor.B})";
            ApplyToPreview();
        }
        Show();
        Activate();
    }

    private void ApplyToPreview()
    {
        SaveUiToConfig();
        _preview?.ApplyConfig(AppConfig.Current);
        _status.Text = $"区域 {AppConfig.Current.X},{AppConfig.Current.Y} {AppConfig.Current.Width}x{AppConfig.Current.Height} @ {AppConfig.Current.Fps} FPS / 阈值 {AppConfig.Current.ColorThreshold}";
    }

    private void StartPreview()
    {
        SaveUiToConfig();

        _preview ??= new PreviewForm();
        _preview.ApplyConfig(AppConfig.Current);
        _preview.Show();
        _preview.BringToFront();

        _start.Enabled = false;
        _stop.Enabled = true;
        _status.Text = "镜像运行中。把预览窗交给 SteamVR / OVR Toolkit / Desktop+ 去抓即可。";
    }

    private void StopPreview()
    {
        _preview?.Hide();
        _start.Enabled = true;
        _stop.Enabled = false;
        _status.Text = "已停止";
    }
}
