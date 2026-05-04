namespace KugouLyricsMirror;

internal sealed class WindowInfo
{
    public IntPtr Hwnd { get; init; }
    public string Title { get; init; } = "";
    public string ClassName { get; init; } = "";
    public bool IsVisible { get; init; }
    public Rectangle Bounds { get; init; }
    public IntPtr ExtendedStyle { get; init; }

    public override string ToString()
    {
        var title = string.IsNullOrWhiteSpace(Title) ? "(无标题)" : Title;
        var visible = IsVisible ? "可见" : "隐藏";
        return $"{title} | {ClassName} | 0x{Hwnd.ToInt64():X} | {visible} | {Bounds.X},{Bounds.Y} {Bounds.Width}x{Bounds.Height} | ex=0x{ExtendedStyle.ToInt64():X}";
    }
}
