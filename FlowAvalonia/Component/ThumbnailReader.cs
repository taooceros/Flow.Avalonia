using System;
using System.Drawing;
using System.Drawing.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace FlowAvalonia.Component
{
    [Flags]
    public enum ThumbnailOptions
    {
        None = 0x00,
        BiggerSizeOk = 0x01,
        InMemoryOnly = 0x02,
        IconOnly = 0x04,
        ThumbnailOnly = 0x08,
        InCacheOnly = 0x10,
    }

    public static class WindowsThumbnailProvider
    {
        public static Bitmap? GetThumbnail(string filePath, int width, int height, ThumbnailOptions options)
        {
            var icon = Icon.ExtractAssociatedIcon(filePath);
            if (icon == null)
            {
                return null;
            }

            var bitmapTmp = icon.ToBitmap();
            var bitmapdata = bitmapTmp.LockBits(new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var bitmap1 = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Unpremul,
                bitmapdata.Scan0,
                new Avalonia.PixelSize(bitmapdata.Width, bitmapdata.Height),
                new Avalonia.Vector(96, 96),
                bitmapdata.Stride);
            bitmapTmp.UnlockBits(bitmapdata);
            bitmapTmp.Dispose();
            return bitmap1;
        }
    }
}