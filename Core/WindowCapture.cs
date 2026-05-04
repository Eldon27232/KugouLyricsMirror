using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KugouLyricsMirror;

internal static class WindowCapture
{
    public static bool TryGetWindowBounds(IntPtr hWnd, out Rectangle bounds, out string? errorMessage)
    {
        bounds = Rectangle.Empty;
        errorMessage = string.Empty;

        if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd))
        {
            errorMessage = "源窗口无效或已关闭";
            return false;
        }

        var hr = NativeMethods.DwmGetWindowAttribute(
            hWnd,
            NativeMethods.DWMWA_EXTENDED_FRAME_BOUNDS,
            out var dwmRect,
            Marshal.SizeOf<RECT>());

        if (hr == 0 && dwmRect.Width > 0 && dwmRect.Height > 0)
        {
            bounds = dwmRect.ToRectangle();
            return true;
        }

        if (NativeMethods.GetWindowRect(hWnd, out var rect) && rect.Width > 0 && rect.Height > 0)
        {
            bounds = rect.ToRectangle();
            return true;
        }

        errorMessage = "无法读取源窗口矩形";
        return false;
    }

    public static bool TryCapture(IntPtr hWnd, out Bitmap? bitmap, out Rectangle bounds, out string? errorMessage)
    {
        bitmap = null;
        bounds = Rectangle.Empty;
        errorMessage = string.Empty;

        if (!TryGetWindowBounds(hWnd, out bounds, out errorMessage))
            return false;

        bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        var hdc = g.GetHdc();
        try
        {
            const uint PW_RENDERFULLCONTENT = 0x00000002;
            if (NativeMethods.PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT))
                return true;
        }
        finally
        {
            g.ReleaseHdc(hdc);
        }

        if (TryCaptureWithWindowDc(hWnd, bitmap, bounds, out errorMessage))
            return true;

        bitmap.Dispose();
        bitmap = null;
        return false;
    }

    private static bool TryCaptureWithWindowDc(IntPtr hWnd, Bitmap bitmap, Rectangle bounds, out string? errorMessage)
    {
        errorMessage = string.Empty;
        var srcDc = NativeMethods.GetWindowDC(hWnd);
        if (srcDc == IntPtr.Zero)
        {
            errorMessage = "GetWindowDC 失败";
            return false;
        }

        using var g = Graphics.FromImage(bitmap);
        var destDc = g.GetHdc();
        try
        {
            const int SRCCOPY = 0x00CC0020;
            if (NativeMethods.BitBlt(destDc, 0, 0, bounds.Width, bounds.Height, srcDc, 0, 0, SRCCOPY))
                return true;

            errorMessage = "PrintWindow 和 GetWindowDC + BitBlt 均失败";
            return false;
        }
        finally
        {
            g.ReleaseHdc(destDc);
            _ = NativeMethods.ReleaseDC(hWnd, srcDc);
        }
    }
}
