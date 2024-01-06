using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RegulSaveCleaner.Core.Converters;

public static class FontNameConverters
{
    /// <summary>
    /// Gets a value converter that converts a name to a <see cref="FontFamily"/> instance. 
    /// </summary>
    /// <remarks>
    /// The converter takes a string input, attempts to parse it to a <see cref="FontFamily"/>. If the name
    /// is null or cannot be parsed, it returns the default <see cref="FontFamily"/>.
    /// </remarks>
    /// <value>
    /// The value converter that converts a name to a <see cref="FontFamily"/> instance.
    /// </value>
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