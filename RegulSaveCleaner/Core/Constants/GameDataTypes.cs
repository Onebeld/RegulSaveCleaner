using RegulSaveCleaner.Structures;

namespace RegulSaveCleaner.Core.Constants;

public static class GameDataTypes
{
    public static readonly GameDataType FamilyPortraits = new()
    {
        ResourceTypes = new uint[]
        {
            0x6B6D837E,
            0x6B6D837D,
            0x6B6D837F
        }
    };

    public static readonly GameDataType GeneratedImages = new()
    {
        ResourceTypes = new uint[]
        {
            0x00B2D882
        },

        ResourceGroups = new uint[]
        {
            0x24B9FCA,
            0x2722299,
            0x2BD69A0,
            0x0E9928A8
        }
    };

    public static readonly GameDataType Photos = new()
    {
        ResourceTypes = new uint[]
        {
            0xB2D882
        },

        ResourceGroups = new uint[]
        {
            0x269D005
        }
    };

    public static readonly GameDataType Textures = new()
    {
        ResourceTypes = new uint[]
        {
            0xB2D882
        },

        ResourceGroups = new uint[]
        {
            0xB0C507, 
            0x1, 
            0xBC1E4C, 
            0x1DA7E9, 
            0xFAE172, 
            0xBC1E54, 
            0x7E7555
        }
    };

    public static readonly GameDataType LotThumbnails = new()
    {
        ResourceTypes = new[]
        {
            0xD84E7FC6
        }
    };

    public static readonly GameDataType SimPortraits = new()
    {
        ResourceTypes = new uint[]
        {
            0x580A2CD, 
            0x580A2CE, 
            0x580A2CF
        }
    };
}