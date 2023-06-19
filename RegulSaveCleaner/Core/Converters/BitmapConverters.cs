using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace RegulSaveCleaner.Core.Converters;

public static class BitmapConverters
{
    public static readonly IValueConverter Compress =
        new FuncValueConverter<Bitmap, Bitmap>(x =>
        {
            if (x is null) throw new NullReferenceException();
            
            if (x.PixelSize.Width > 128 || x.PixelSize.Height > 128)
                return x.CreateScaledBitmap(new PixelSize(128, 128), BitmapInterpolationMode.LowQuality);

            return x.CreateScaledBitmap(x.PixelSize, BitmapInterpolationMode.LowQuality);
        });
}