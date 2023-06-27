using Avalonia.Collections;
using PleasantUI;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.Views.Windows;

namespace RegulSaveCleaner.ViewModels.Windows;

public class OldProhibitedListsViewModel : ViewModelBase
{
    private readonly GameSave _saveFile;
    private GameSaveResource? _selectedGameGameSaveResource;

    public GameSaveResource? SelectedGameSaveResources
    {
        get => _selectedGameGameSaveResource;
        set => RaiseAndSet(ref _selectedGameGameSaveResource, value);
    }

    public AvaloniaList<GameSaveResource> GameSaveResourcesList { get; } = new();

    public OldProhibitedListsViewModel(GameSave saveFile, IEnumerable<string> saveNames)
    {
        _saveFile = saveFile;
        foreach (GameSaveResource saveResources in RegulSettings.Instance.GameSaveResources.Where(saveResources => saveNames.All(x => x != saveResources.Id)))
            GameSaveResourcesList.Add(saveResources);
    }

    public void CloseWithSave(OldProhibitedListsWindow window)
    {
        if (SelectedGameSaveResources is null) return;
        
        GameSaveResource? saveResources = RegulSettings.Instance.GameSaveResources.FirstOrDefault(x => x.Id == _saveFile.Name);

        if (saveResources is null)
        {
            saveResources = new GameSaveResource
            {
                Id = _saveFile.Name,
                ProhibitedResources = new AvaloniaList<ProhibitedResource>(SelectedGameSaveResources.ProhibitedResources)
            };
            RegulSettings.Instance.GameSaveResources.Add(saveResources);
        }
        else
            saveResources.ProhibitedResources.AddRange(SelectedGameSaveResources.ProhibitedResources);

        RegulSettings.Instance.GameSaveResources.Remove(SelectedGameSaveResources);
        
        window.Close(true);
    }

    public void Close(OldProhibitedListsWindow window) => window.Close(false);
}