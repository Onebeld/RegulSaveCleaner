using Avalonia;
using Avalonia.Media.Imaging;

namespace RegulSaveCleaner.Core.Extensions;

public static class BitmapExtensions
{
    private const int imageSizeCompressed = 128;
    
    /// <summary>
    /// Compresses the image
    /// </summary>
    /// <param name="bitmap">Original image</param>
    /// <returns>Compressed image</returns>
    public static Bitmap Compress(this Bitmap bitmap)
    {
        if (bitmap.PixelSize.Width > imageSizeCompressed || bitmap.PixelSize.Height > imageSizeCompressed)
            return bitmap.CreateScaledBitmap(new PixelSize(imageSizeCompressed, imageSizeCompressed), BitmapInterpolationMode.LowQuality);

        return bitmap;
    }
}