using Avalonia.Media.Imaging;

namespace RegulSaveCleaner.Structures;

public class ImageResource
{
    public uint Type { get; init; }
    public uint Group { get; init; }
    public ulong Instance { get; init; }
    
    public Bitmap? Image { get; init; }
    public Bitmap? CompressedImage { get; set; }
}