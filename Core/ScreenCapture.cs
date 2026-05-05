using System.Drawing.Drawing2D;

namespace KugouLyricsMirror;

internal static class ScreenCapture
{
    public static void CopyScreenAreaToBitmap(Bitmap bitmap, Rectangle sourceRect)
    {
        using var g = Graphics.FromImage(bitmap);
        using var screenG = Graphics.FromHwnd(IntPtr.Zero);

        g.CompositingMode = CompositingMode.SourceCopy;
        IntPtr hdcDest = g.GetHdc();
        IntPtr hdcSrc = screenG.GetHdc();

        try
        {
            const int SRCCOPY = 0x00CC0020;
            const int CAPTUREBLT = 0x40000000;

            _ = NativeMethods.BitBlt(
                hdcDest,
                0,
                0,
                sourceRect.Width,
                sourceRect.Height,
                hdcSrc,
                sourceRect.X,
                sourceRect.Y,
                SRCCOPY | CAPTUREBLT);
        }
        finally
        {
            screenG.ReleaseHdc(hdcSrc);
            g.ReleaseHdc(hdcDest);
        }
    }

    public static Color GetColorAt(Point screenPoint)
    {
        using var bmp = new Bitmap(1, 1);
        CopyScreenAreaToBitmap(bmp, new Rectangle(screenPoint.X, screenPoint.Y, 1, 1));
        return bmp.GetPixel(0, 0);
    }

    public static Color EstimateBackgroundColor(Bitmap bitmap)
    {
        if (bitmap.Width <= 0 || bitmap.Height <= 0)
            return Color.Black;

        var buckets = new Dictionary<int, int>();
        var maxX = bitmap.Width - 1;
        var maxY = bitmap.Height - 1;
        var stepX = Math.Max(1, bitmap.Width / 24);
        var stepY = Math.Max(1, bitmap.Height / 12);

        void AddSample(int x, int y)
        {
            var color = bitmap.GetPixel(Math.Clamp(x, 0, maxX), Math.Clamp(y, 0, maxY));
            var key = QuantizeColor(color);
            buckets[key] = buckets.TryGetValue(key, out var count) ? count + 1 : 1;
        }

        for (var x = 0; x <= maxX; x += stepX)
        {
            AddSample(x, 0);
            AddSample(x, maxY);
        }

        for (var y = 0; y <= maxY; y += stepY)
        {
            AddSample(0, y);
            AddSample(maxX, y);
        }

        AddSample(0, 0);
        AddSample(maxX, 0);
        AddSample(0, maxY);
        AddSample(maxX, maxY);

        if (buckets.Count == 0)
            return Color.Black;

        var best = buckets.OrderByDescending(pair => pair.Value).First().Key;
        return Color.FromArgb(
            ((best >> 16) & 0xFF) + 4,
            ((best >> 8) & 0xFF) + 4,
            (best & 0xFF) + 4);
    }

    private static int QuantizeColor(Color color)
    {
        return (color.R & 0xF8) << 16
            | (color.G & 0xF8) << 8
            | (color.B & 0xF8);
    }
}
