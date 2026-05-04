using System.Text;

namespace KugouLyricsMirror;

internal static class WindowEnumerator
{
    public static IReadOnlyList<WindowInfo> EnumerateTopLevelWindows()
    {
        var windows = new List<WindowInfo>();

        NativeMethods.EnumWindows((hWnd, lParam) =>
        {
            var title = GetWindowText(hWnd);
            var className = GetClassName(hWnd);

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(className))
                return true;

            _ = NativeMethods.GetWindowRect(hWnd, out var rect);
            windows.Add(new WindowInfo
            {
                Hwnd = hWnd,
                Title = title,
                ClassName = className,
                IsVisible = NativeMethods.IsWindowVisible(hWnd),
                Bounds = rect.ToRectangle(),
                ExtendedStyle = NativeMethods.GetWindowLongPtr(hWnd, NativeMethods.GWL_EXSTYLE)
            });

            return true;
        }, 0);

        return windows
            .OrderByDescending(w => w.IsVisible)
            .ThenBy(w => string.IsNullOrWhiteSpace(w.Title))
            .ThenBy(w => w.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string GetWindowText(IntPtr hWnd)
    {
        var length = Math.Max(256, NativeMethods.GetWindowTextLength(hWnd) + 1);
        var buffer = new StringBuilder(length);
        _ = NativeMethods.GetWindowText(hWnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string GetClassName(IntPtr hWnd)
    {
        var buffer = new StringBuilder(256);
        _ = NativeMethods.GetClassName(hWnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }
}
