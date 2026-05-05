namespace KugouLyricsMirror;

internal sealed class Config
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Width { get; set; } = 600;
    public int Height { get; set; } = 120;
    public int Fps { get; set; } = 20;
    public int ColorThreshold { get; set; } = 36;
    public int KeyColorArgb { get; set; } = Color.Black.ToArgb();
    public int? RegionKeyColorArgb { get; set; } = null;
    public bool RegionAutoKeyColor { get; set; } = true;
    public int? WindowChromaFillColorArgb { get; set; } = null;
    public bool WindowFollowSourceWindow { get; set; } = false;
    public bool TopMost { get; set; } = true;
    public bool ExcludeFromCapture { get; set; } = false;
    public string CaptureMode { get; set; } = KugouLyricsMirror.CaptureMode.DwmWindow;
    public long SourceWindowHandle { get; set; } = 0;
    public bool BackdropVisible { get; set; } = false;
    public bool BackdropLocked { get; set; } = false;
    public int BackdropX { get; set; } = 0;
    public int BackdropY { get; set; } = 0;
    public int BackdropWidth { get; set; } = 600;
    public int BackdropHeight { get; set; } = 120;
    public int? PreviewX { get; set; } = null;
    public int? PreviewY { get; set; } = null;

    public Color KeyColor
    {
        get => RegionKeyColor;
        set => RegionKeyColor = value;
    }

    public Color RegionKeyColor
    {
        get => Color.FromArgb(RegionKeyColorArgb ?? KeyColorArgb);
        set => RegionKeyColorArgb = value.ToArgb();
    }

    public Color WindowChromaFillColor
    {
        get => Color.FromArgb(WindowChromaFillColorArgb ?? Color.Lime.ToArgb());
        set => WindowChromaFillColorArgb = value.ToArgb();
    }
}
