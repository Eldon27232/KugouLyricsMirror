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
    public bool TopMost { get; set; } = true;
    public bool ExcludeFromCapture { get; set; } = false;

    public Color KeyColor
    {
        get => Color.FromArgb(KeyColorArgb);
        set => KeyColorArgb = value.ToArgb();
    }
}
