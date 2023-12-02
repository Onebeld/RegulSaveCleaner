using Avalonia.Collections;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Windows;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Core.Extensions;
using RegulSaveCleaner.Structures;

namespace RegulSaveCleaner.ViewModels.Windows;

public class GameSavesTransferViewModel : ViewModelBase
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current!;
    
    private bool _isLoadingSaves;
    
    private bool _sortByAlphabet;
    private bool _sortByDate = true;
    
    private AvaloniaList<GameSave> _selectedGameSaves = new();
    private AvaloniaList<GameSave> _gameSaves = new();
    
    public bool SortByAlphabet
    {
        get => _sortByAlphabet;
        set
        {
            RaiseAndSet(ref _sortByAlphabet, value);
            
            if (value)
                SortGameSaves();
        }
    }

    public bool SortByDate
    {
        get => _sortByDate;
        set
        {
            RaiseAndSet(ref _sortByDate, value);
            
            if (value)
                SortGameSaves();
        }
    }

    public bool IsLoadingSaves
    {
        get => _isLoadingSaves;
        set => RaiseAndSet(ref _isLoadingSaves, value);
    }

    public AvaloniaList<GameSave> GameSaves
    {
        get => _gameSaves;
        set => RaiseAndSet(ref _gameSaves, value);
    }
    
    public AvaloniaList<GameSave> SelectedGameSaves
    {
        get => _selectedGameSaves;
        set => RaiseAndSet(ref _selectedGameSaves, value);
    }

    public async void LoadOldSaves()
    {
        if (!Directory.Exists(RegulSettings.Instance.PathToFolderWithOldSaves))
            Directory.CreateDirectory(RegulSettings.Instance.PathToFolderWithOldSaves);

        IsLoadingSaves = true;
        
        foreach (string directory in Directory.EnumerateDirectories(RegulSettings.Instance.PathToFolderWithOldSaves, "*.sims3"))
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (string file in Directory.EnumerateFiles(directory, "*.nhd", SearchOption.TopDirectoryOnly))
                    {
                        GameSave? currentGameSave = GameSaves.FirstOrDefault(x => x.Directory == directory);

                        GameSaveData gameSaveData = new(file);
                            
                        if (currentGameSave is not null)
                        {
                            if (gameSaveData.FamilyIcon is not null)
                            {
                                _synchronizationContext.Send(_ =>
                                {
                                    currentGameSave.ImageOfFamily = gameSaveData.FamilyIcon;
                                    currentGameSave.ImageInstance = gameSaveData.ImgInstance;
                                }, "");
                            }
                                
                            continue;
                        }
                        
                        GameSave gameSave = GameSave.Create(directory, gameSaveData, _synchronizationContext);
                        
                        _synchronizationContext.Send(_ =>
                        {
                            GameSaves.Add(gameSave);
                        }, "");
                    }
                });
            }
            catch (Exception e)
            {
                await MessageBox.Show(App.MainWindow, "AnErrorHasOccurred", string.Empty, additionalText: e.ToString());
            }
        }
        
        IsLoadingSaves = false;
        
        SortGameSaves();
    }
    
    public void SelectAllSaves()
    {
        SelectedGameSaves.Clear();
        SelectedGameSaves.AddRange(GameSaves);
    }

    public void CancelAllSaves() => SelectedGameSaves.Clear();
    
    private void SortGameSaves()
    {
        List<GameSave> selectedGameSave = SelectedGameSaves.ToList();
        List<GameSave> gameSaves = GameSaves.ToList();

        gameSaves = SortByAlphabet ? gameSaves.OrderBy(x => x.Name).ToList() : gameSaves.OrderByDescending(x => x.LastSaveTime).ToList();
        
        GameSaves.Clear();
        GameSaves.AddRange(gameSaves);
        
        SelectedGameSaves.Clear();
        SelectedGameSaves.AddRange(selectedGameSave);
    }
    
    public async void CloseWithResult(ContentDialog window)
    {
        IsLoadingSaves = true;
        
        await Task.Run(() =>
        {
            foreach (GameSave gameSave in SelectedGameSaves)
            {
                DirectoryManager.Copy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToSaves, gameSave.Name + ".sims3"), true);
                Directory.Delete(gameSave.Directory, true);
            }
        });

        IsLoadingSaves = false;
        
        window.Close(true);
    }

    public void Close(ContentDialog window) => window.Close(false);
}