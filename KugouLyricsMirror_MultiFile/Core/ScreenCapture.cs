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
}
