using System.Drawing.Imaging;

namespace KugouLyricsMirror;

internal static class ChromaKeyProcessor
{
    public static void Apply(Bitmap bmp, Color keyColor, int threshold, Color transparentKey)
    {
        if (bmp.Width <= 0 || bmp.Height <= 0) return;

        threshold = Math.Clamp(threshold, 0, 255);

        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        try
        {
            var bytesPerPixel = Image.GetPixelFormatSize(data.PixelFormat) / 8;
            if (bytesPerPixel is not 3 and not 4)
                return;

            var keyR = keyColor.R;
            var keyG = keyColor.G;
            var keyB = keyColor.B;
            var transparentR = transparentKey.R;
            var transparentG = transparentKey.G;
            var transparentB = transparentKey.B;
            var transparentA = transparentKey.A;

            unsafe
            {
                var stride = data.Stride;
                var strideLength = Math.Abs(stride);
                var scan0 = (byte*)data.Scan0;

                for (var y = 0; y < bmp.Height; y++)
                {
                    var row = stride > 0
                        ? scan0 + y * stride
                        : scan0 + (bmp.Height - 1 - y) * strideLength;

                    for (var x = 0; x < bmp.Width; x++)
                    {
                        var pixel = row + x * bytesPerPixel;
                        var b = pixel[0];
                        var g = pixel[1];
                        var r = pixel[2];

                        if (Math.Abs(r - keyR) <= threshold
                            && Math.Abs(g - keyG) <= threshold
                            && Math.Abs(b - keyB) <= threshold)
                        {
                            pixel[0] = transparentB;
                            pixel[1] = transparentG;
                            pixel[2] = transparentR;
                            if (bytesPerPixel == 4)
                                pixel[3] = transparentA;
                        }
                    }
                }
            }
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }
}
