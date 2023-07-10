using Avalonia.Media.Imaging;

namespace RegulSaveCleaner.Structures;

public class ImageResource
{
    public uint Type { get; set; }
    public uint Group { get; set; }
    public ulong Instance { get; set; }
    
    public Bitmap? Image { get; set; }
    public Bitmap? CompressedImage { get; set; }
}