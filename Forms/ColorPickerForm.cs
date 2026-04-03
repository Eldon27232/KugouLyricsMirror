namespace KugouLyricsMirror;

internal sealed class ColorPickerForm : Form
{
    private Color _currentColor = Color.Black;

    private readonly Label _info = new()
    {
        AutoSize = true,
        BackColor = Color.FromArgb(180, 0, 0, 0),
        ForeColor = Color.White,
        Padding = new Padding(8),
        Text = "移动鼠标选择背景色，左键确认，ESC 取消"
    };

    private readonly Panel _swatch = new()
    {
        Width = 48,
        Height = 48,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.Black
    };

    public Color SelectedColor { get; private set; } = Color.Black;

    public ColorPickerForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        Bounds = SystemInformation.VirtualScreen;
        StartPosition = FormStartPosition.Manual;
        Location = SystemInformation.VirtualScreen.Location;
        TopMost = true;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        Cursor = Cursors.Cross;
        BackColor = Color.Black;
        Opacity = 0.01;
        KeyPreview = true;

        var host = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(20, 20)
        };
        host.Controls.Add(_info);
        host.Controls.Add(_swatch);
        Controls.Add(host);

        MouseMove += (_, _) => UpdateColorAtCursor();
        MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            UpdateColorAtCursor();
            SelectedColor = _currentColor;
            DialogResult = DialogResult.OK;
            Close();
        };

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        };
    }

    private void UpdateColorAtCursor()
    {
        var pos = Cursor.Position;
        _currentColor = ScreenCapture.GetColorAt(pos);
        _swatch.BackColor = _currentColor;
        _info.Text = $"移动鼠标选择背景色，左键确认，ESC 取消\n当前: RGB({_currentColor.R}, {_currentColor.G}, {_currentColor.B})";
    }
}
