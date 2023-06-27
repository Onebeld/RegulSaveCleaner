using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RegulSaveCleaner.Core.Converters;

public class UnknownIconToMarginConverter : IValueConverter
{
    public static readonly UnknownIconToMarginConverter Instance = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DrawingImage image)
            return AvaloniaProperty.UnsetValue;

        return Equals(image, App.GetResource<DrawingImage>("UnknownIcon")) ? Thickness.Parse("30") : Thickness.Parse("0");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}