using System.Text;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RegulSaveCleaner.S3PI;
using RegulSaveCleaner.S3PI.Interfaces;
using RegulSaveCleaner.S3PI.Package;

namespace RegulSaveCleaner.Core;

public class GameSaveData
{
    public IImage? FamilyIcon;

    public string WorldName = string.Empty;
    public ulong ImgInstance;
    
    public GameSaveData(string strPath)
    {
        Package package = (Package)Package.OpenPackage(Path.Combine(Path.GetDirectoryName(strPath)!, "Meta.data"));
        IResourceIndexEntry rie = package.Find(r => r.ResourceType == 0x628A788F);
        Extract(WrapperDealer.GetResource(package, rie).Stream, strPath);
    }

    private void Extract(Stream s, string nhdPath)
    {
        using FastBinaryReader fastBinaryReader = new(s);
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

        // Skip family description
        int numSymbolForFamilyDescription = checked(fastBinaryReader.ReadInt32() - 1);
        for (int index = 0; index <= numSymbolForFamilyDescription; index++) 
            fastBinaryReader.BaseStream.Position += 2;
        
        // Get image instance
        ImgInstance = fastBinaryReader.ReadUInt64();

        IPackage pkg = Package.OpenPackage(nhdPath);
        IResourceIndexEntry? rie = null;
        if (ImgInstance != 0)
            foreach (IResourceIndexEntry resource in pkg.GetResourceList)
            {
                if (resource.Instance == ImgInstance && resource.ResourceType == 1802339198U)
                {
                    rie = resource;
                    break;
                }
            }

        FamilyIcon = rie != null ? new Bitmap(WrapperDealer.GetResource(pkg, rie).Stream) : null;
    }
}