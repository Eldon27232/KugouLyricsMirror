namespace KugouLyricsMirror;

internal static class ChromaKeyProcessor
{
    public static void Apply(Bitmap bmp, Color keyColor, int threshold, Color transparentKey)
    {
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (IsNearColor(c, keyColor, threshold))
                {
                    bmp.SetPixel(x, y, transparentKey);
                }
            }
        }
    }

    private static bool IsNearColor(Color a, Color b, int threshold)
    {
        return Math.Abs(a.R - b.R) <= threshold
            && Math.Abs(a.G - b.G) <= threshold
            && Math.Abs(a.B - b.B) <= threshold;
    }
}
