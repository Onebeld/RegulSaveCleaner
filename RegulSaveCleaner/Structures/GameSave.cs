using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI;
using RegulSaveCleaner.Core;

namespace RegulSaveCleaner.Structures;

public class GameSave : ViewModelBase
{
    private IImage? _imageOfFamily;
    
    public string Name { get; }
    
    public string Directory { get; }
    
    public required ulong ImageInstance { get; set; }

    public required string WorldName { get; set; }

    public required string Description { get; set; }
    
    public required DateTime LastSaveTime { get; set; }

    public required IImage? ImageOfFamily
    {
        get => _imageOfFamily;
        set => RaiseAndSet(ref _imageOfFamily, value);
    }

    private GameSave(string directory)
    {
        Directory = directory;
        Name = new DirectoryInfo(directory).Name.Replace(".sims3", "");
    }

    public static GameSave Create(string directory, GameSaveData gameSaveData, SynchronizationContext context)
    {
        IImage? image = null;

        if (gameSaveData.FamilyIcon is null)
        {
            context.Send(_ =>
            {
                image = (DrawingImage)Application.Current!.FindResource("UnknownIcon")!;
            }, "");
        }
        else image = gameSaveData.FamilyIcon;
        
        GameSave gameSave = new(directory)
        {
            Description = gameSaveData.Description,
            WorldName = gameSaveData.WorldName,
            ImageInstance = gameSaveData.ImgInstance,
            ImageOfFamily = image,
            LastSaveTime = gameSaveData.LastSaveTime
        };

        return gameSave;
    }
}