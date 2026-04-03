namespace KugouLyricsMirror;

internal sealed class RegionPickerForm : Form
{
    private Point _start;
    private Point _end;
    private bool _dragging;

    public Rectangle SelectedRegion { get; private set; }

    public RegionPickerForm()
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
        Opacity = 0.20;
        KeyPreview = true;

        MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _start = PointToScreen(e.Location);
            _end = _start;
            Invalidate();
        };

        MouseMove += (_, e) =>
        {
            if (!_dragging) return;
            _end = PointToScreen(e.Location);
            Invalidate();
        };

        MouseUp += (_, e) =>
        {
            if (!_dragging || e.Button != MouseButtons.Left) return;
            _dragging = false;
            _end = PointToScreen(e.Location);
            var rect = Normalize(_start, _end);
            if (rect.Width >= 10 && rect.Height >= 10)
            {
                SelectedRegion = rect;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_dragging) return;

        var rect = Normalize(PointToClient(_start), PointToClient(_end));
        using var pen = new Pen(Color.Lime, 2);
        using var fill = new SolidBrush(Color.FromArgb(50, Color.Lime));
        e.Graphics.FillRectangle(fill, rect);
        e.Graphics.DrawRectangle(pen, rect);
    }

    private static Rectangle Normalize(Point a, Point b)
    {
        int x = Math.Min(a.X, b.X);
        int y = Math.Min(a.Y, b.Y);
        int w = Math.Abs(a.X - b.X);
        int h = Math.Abs(a.Y - b.Y);
        return new Rectangle(x, y, w, h);
    }
}
