using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace RegulSaveCleaner.Core.Converters;

public class FoldingConverter : IValueConverter
{
    public static readonly FoldingConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double num || parameter is not string stringNum)
            return AvaloniaProperty.UnsetValue;

        if (double.TryParse(stringNum, NumberStyles.Any, CultureInfo.InvariantCulture, out double sum))
            return num + sum;

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}