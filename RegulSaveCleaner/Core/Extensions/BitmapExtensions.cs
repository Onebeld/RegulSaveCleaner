using Avalonia;
using Avalonia.Media.Imaging;

namespace RegulSaveCleaner.Core.Extensions;

public static class BitmapExtensions
{
    public static Bitmap Compress(this Bitmap bitmap)
    {
        if (bitmap.PixelSize.Width > 128 || bitmap.PixelSize.Height > 128)
            return bitmap.CreateScaledBitmap(new PixelSize(128, 128), BitmapInterpolationMode.LowQuality);

        return bitmap.CreateScaledBitmap(bitmap.PixelSize, BitmapInterpolationMode.LowQuality);
    }
}