using Avalonia.Media;
using PleasantUI;

namespace RegulSaveCleaner.Structures;

public class GameSave : ViewModelBase
{
    private readonly Lazy<string> _name;
    private readonly Lazy<string> _location;
    private IImage? _imageOfFamily;

    public IImage? ImageOfFamily
    {
        get => _imageOfFamily;
        set => RaiseAndSet(ref _imageOfFamily, value);
    }
    public ulong ImageInstance { get; set; }

    public string Directory { get; }

    public string WorldName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public DateTime LastSaveTime { get; set; }

    public string Name => _name.Value;
    public string Location => _location.Value;

    public GameSave(string directory)
    {
        Directory = directory;

        _name = new Lazy<string>(() => new DirectoryInfo(Directory).Name[..^".sims3".Length]);
        _location = new Lazy<string>(() =>
        {
            string str = "";
            string[] files = System.IO.Directory.GetFiles(Directory, "*.dat", SearchOption.AllDirectories);
            if (0 < files.Length)
                str = File.ReadAllText(files[0]).Split('_')[0];
            return str;
        });
    }
}