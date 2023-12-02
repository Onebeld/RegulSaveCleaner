using System.Text;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RegulSaveCleaner.S3PI;
using RegulSaveCleaner.S3PI.Package;

namespace RegulSaveCleaner.Core;

public class GameSaveData
{
    /// <summary>
    /// The image of the family that is noted in the save
    /// </summary>
    public IImage? FamilyIcon;

    /// <summary>
    /// The name of the world that is noted in the save
    /// </summary>
    public string WorldName = string.Empty;
    
    /// <summary>
    /// Description of save
    /// </summary>
    public string Description = string.Empty;
    
    /// <summary>
    /// The instance number of the image of family that is marked in the save
    /// </summary>
    public ulong ImgInstance;
    
    /// <summary>
    /// Time of last save change
    /// </summary>
    public DateTime LastSaveTime;
    
    public GameSaveData(string strPath)
    {
        string metaData = Path.Combine(Path.GetDirectoryName(strPath)!, "Meta.data");
        
        LastSaveTime = new FileInfo(metaData).LastWriteTime;
        
        Package package = Package.OpenPackage(metaData);
        ResourceIndexEntry rie = package.Find(r => r.ResourceType == 0x628A788F);
        Extract(WrapperDealer.GetResource(package, rie).Stream, strPath);
        
        Package.ClosePackage(package);
    }

    /// <summary>
    /// Obtains the necessary information from the save
    /// </summary>
    /// <param name="stream">The resource content as a Stream</param>
    /// <param name="nhdPath">The path to save</param>
    private void Extract(Stream stream, string nhdPath)
    {
        using FastBinaryReader fastBinaryReader = new(stream);
        StringBuilder stringBuilder = new();
        
        fastBinaryReader.BaseStream.Position += 4;

        // Get world name
        int numSymbolForWorldName = checked(fastBinaryReader.ReadInt32() - 1);
        for (int index = 0; index <= numSymbolForWorldName; index++) 
            stringBuilder.Append((char)fastBinaryReader.ReadInt16());

        // Get family name
        int numSymbolForFamily = checked(fastBinaryReader.ReadInt32() - 1);
        if (numSymbolForFamily != -1) stringBuilder.Append(" | ");
        
        for (int index = 0; index <= numSymbolForFamily; index++) 
            stringBuilder.Append((char)fastBinaryReader.ReadInt16());

        WorldName = stringBuilder.ToString();

        // Get family description
        string description = string.Empty;
        int numSymbolForFamilyDescription = checked(fastBinaryReader.ReadInt32() - 1);
        for (int index = 0; index <= numSymbolForFamilyDescription; index++)
            description += (char)fastBinaryReader.ReadInt16();

        Description = description;
        
        // Get image instance
        ImgInstance = fastBinaryReader.ReadUInt64();

        Package pkg = Package.OpenPackage(nhdPath);
        ResourceIndexEntry? rie = null;
        if (ImgInstance != 0)
            foreach (ResourceIndexEntry resource in pkg.GetResourceList)
            {
                if (resource.Instance != ImgInstance || resource.ResourceType != 1802339198U)
                    continue;

                rie = resource;
                break;
            }

        FamilyIcon = rie != null ? new Bitmap(WrapperDealer.GetResource(pkg, rie).Stream) : null;
        Package.ClosePackage(pkg);
    }
}