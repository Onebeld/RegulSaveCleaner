using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace RegulSaveCleaner.Core.Converters;

public class UIntToHexConverter : IValueConverter
{
    public static readonly UIntToHexConverter Instance = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is not uint uintColor ? AvaloniaProperty.UnsetValue : $"#{uintColor.ToString("x8").ToUpper()}";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}