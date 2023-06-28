using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RegulSaveCleaner.Core.Converters;

public static class FontNameConverters
{
    public static IValueConverter NameToFontFamily => 
        new FuncValueConverter<string, FontFamily>(x =>
        {
            try
            {
                if (x != null)
                    return FontFamily.Parse(x);
            }
            catch
            {
                return FontFamily.Default;
            }
            
            return FontFamily.Default;
        });
}