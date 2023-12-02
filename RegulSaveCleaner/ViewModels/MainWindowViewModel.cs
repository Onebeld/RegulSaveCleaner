using System.Diagnostics;
using System.Text;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using PleasantUI;
using PleasantUI.Windows;
using RegulSaveCleaner.Controls;
using RegulSaveCleaner.S3PI.Package;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Core.Constants;
using RegulSaveCleaner.Core.Extensions;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.Views.InformationPages.ClearCache;
using RegulSaveCleaner.Views.InformationPages.ClearSaves;
using RegulSaveCleaner.Views.Windows;

namespace RegulSaveCleaner.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current!;
    
    private bool _inCreatingBackupProcess;
    private bool _inCleaningCacheProcess;
    private bool _inCleaningProcess;
    private bool _isLoadingSaves;
    private bool _canBeCleaningCache;
    private bool _isFoundSaveFolder;
    private bool _isMovingSaves;

    private bool _sortByAlphabet;
    private bool _sortByDate = true;
    
    private AvaloniaList<GameSave> _selectedGameSaves = new();
    private IManagedNotificationManager _notificationManager = null!;

    private string? _currentCleaningSaveName;
    
    /// <summary>
    /// General list of game saves
    /// </summary>
    public AvaloniaList<GameSave> GameSaves { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the current operating system is MacOS.
    /// </summary>
    /// <remarks>
    /// This property checks the compilation flag "OSX" to determine the operating system.
    /// </remarks>
    /// <value>
    /// True if the current environment is MacOS; otherwise, false.
    /// </value>
    public bool IsMacOs
    {
        get
        {
#if OSX
            return true;
#else
            return false;
#endif
        }
    }
    
    /// <summary>
    /// Manager required to show a pop-up notification in the program
    /// </summary>
    public IManagedNotificationManager NotificationManager
    {
        get => _notificationManager;
        set => RaiseAndSet(ref _notificationManager, value);
    }

    /// <summary>
    /// List of selected saves
    /// </summary>
    public AvaloniaList<GameSave> SelectedGameSaves
    {
        get => _selectedGameSaves;
        set => RaiseAndSet(ref _selectedGameSaves, value);
    }
    
    /// <summary>
    /// Indicates whether the program is currently creating backups of saves
    /// </summary>
    public bool InCreatingBackupProcess
    {
        get => _inCreatingBackupProcess;
        set => RaiseAndSet(ref _inCreatingBackupProcess, value);
    }

    /// <summary>
    /// Indicates whether the program is currently clearing the cache
    /// </summary>
    public bool InCleaningCacheProcess
    {
        get => _inCleaningCacheProcess;
        set => RaiseAndSet(ref _inCleaningCacheProcess, value);
    }

    /// <summary>
    /// Indicates whether the program is clearing saves at this point in time
    /// </summary>
    public bool InCleaningProcess
    {
        get => _inCleaningProcess;
        private set => RaiseAndSet(ref _inCleaningProcess, value);
    }
    
    /// <summary>
    /// Indicates whether all saves have been loaded into the program
    /// </summary>
    public bool IsLoadingSaves
    {
        get => _isLoadingSaves;
        set => RaiseAndSet(ref _isLoadingSaves, value);
    }

    /// <summary>
    /// Indicates whether it is allowed to clear the cache
    /// </summary>
    public bool CanBeCleaningCache
    {
        get => _canBeCleaningCache;
        set => RaiseAndSet(ref _canBeCleaningCache, value);
    }
    
    /// <summary>
    /// Indicates if the folder with game saves is found
    /// </summary>
    public bool IsFoundSaveFolder
    {
        get => _isFoundSaveFolder;
        set => RaiseAndSet(ref _isFoundSaveFolder, value);
    }

    public bool IsMovingSaves
    {
        get => _isMovingSaves;
        set => RaiseAndSet(ref _isMovingSaves, value);
    }

    /// <summary>
    /// Specifies alphabetical sorting of the list of saves
    /// </summary>
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

    /// <summary>
    /// Specifying sorting by date of the list of saves
    /// </summary>
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

    /// <summary>
    /// Creates a new main ViewModel object for the main window
    /// </summary>
    public MainWindowViewModel() { }

    /// <summary>
    /// Creates a new ViewModel object for the main window and also creates a notification manager
    /// </summary>
    /// <param name="host">The window for which the notification manager will be installed</param>
    public MainWindowViewModel(TopLevel host)
    {
        _notificationManager = new WindowNotificationManager(host)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
    }

    /// <summary>
    /// Sorts the list of saves
    /// </summary>
    private void SortGameSaves()
    {
        List<GameSave> selectedGameSave = SelectedGameSaves.ToList();
        List<GameSave> gameSaves = GameSaves.ToList();

        if (SortByAlphabet)
            gameSaves = gameSaves.OrderBy(x => x.Name).ToList();
        else
            gameSaves = gameSaves.OrderByDescending(x => x.LastSaveTime).ToList();
        
        GameSaves.Clear();
        GameSaves.AddRange(gameSaves);
        
        SelectedGameSaves.Clear();
        SelectedGameSaves.AddRange(selectedGameSave);
    }

    /// <summary>
    /// Loads all saves into the program
    /// </summary>
    public async Task LoadSaves()
    {
        IsFoundSaveFolder = false;
        IsLoadingSaves = true;
        List<string> saves = SelectedGameSaves.Select(selectedSaveFile => selectedSaveFile.Name).ToList();

        while (true)
        {
            SelectedGameSaves.Clear();
            GameSaves.Clear();

            if (!Directory.Exists(RegulSettings.Instance.PathToTheSims3Document))
            {
                string result = await MessageBox.Show(App.MainWindow, App.GetString("NotFindFolderTheSims3"), App.GetString("NotFindFolderDescription"), MessageBoxButtons.YesNo);

                if (result == "Yes")
                {
                    string? path = await StorageProvider.SelectDirectory(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        RegulSettings.Instance.PathToTheSims3Document = path;
                        RegulSettings.Instance.PathToSaves = Path.Combine(RegulSettings.Instance.PathToTheSims3Document, "Saves");
                    }
                    else
                    {
                        CanBeCleaningCache = false;
                        return;
                    }
                    
                    continue;
                }

                CanBeCleaningCache = false;
                
                break;
            }

            CanBeCleaningCache = true;

            try
            {
                if (!Directory.Exists(RegulSettings.Instance.PathToSaves))
                {
                    string result = await MessageBox.Show(App.MainWindow, App.GetString("SaveFilesNotFound"), App.GetString("NotFindFolderDescription"), MessageBoxButtons.YesNo);

                    if (result == "Yes")
                    {
                        string? path = await StorageProvider.SelectDirectory(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

                        if (!string.IsNullOrWhiteSpace(path))
                            RegulSettings.Instance.PathToSaves = path;
                        else return;
                        
                        continue;
                    }
                    
                    break;
                }
            }
            catch (Exception e)
            {
                await MessageBox.Show(App.MainWindow, App.GetString("AnErrorHasOccurred"), string.Empty, additionalText: e.ToString());
                
                break;
            }

            IsFoundSaveFolder = true;

            foreach (string directory in Directory.EnumerateDirectories(RegulSettings.Instance.PathToSaves, "*.sims3"))
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
                                if (saves.Exists(save => gameSave.Name == save))
                                    SelectedGameSaves.Add(gameSave);
                            }, "");
                        }
                    });
                }
                catch (Exception e)
                {
                    await MessageBox.Show(App.MainWindow, "AnErrorHasOccurred", string.Empty, additionalText: e.ToString());
                }
            }
            
            break;
        }

        IsLoadingSaves = false;
        
        SortGameSaves();

        if (CheckNumberOfSaves())
            await new WarningAboutLargeNumberOfSavesWindow().Show(App.MainWindow);
    }

    /// <summary>
    /// Selects all saves in the list
    /// </summary>
    public void SelectAllSaves()
    {
        SelectedGameSaves.Clear();
        SelectedGameSaves.AddRange(GameSaves);
    }

    /// <summary>
    /// Unselects all saves in the list
    /// </summary>
    public void CancelAllSaves() => SelectedGameSaves.Clear();

    public void SelectAllClearOptions() => ToggleClearOptions(true);

    public void CancelAllClearOptions() => ToggleClearOptions(false);

    public void SelectAllCacheOptions() => ToggleCacheOptions(true);

    public void CancelAllCacheOptions() => ToggleCacheOptions(false);

    public void OpenRemoveFamilyPortraitsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveFamilyPortraitsPage());
    public void OpenRemovePortraitsSimsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemovePortraitsOfSimsPage());
    public void OpenRemoveLotThumbnailsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveLotThumbnailsPage());
    public void OpenRemovePhotosDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemovePhotosPage());
    public void OpenRemoveGeneratedImagesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveGeneratedImagesPage());
    public void OpenRemoveTexturesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveTexturesPage());
    public void OpenRemoveOtherTypesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveOtherTypesPage());
    
    public void OpenCASPartCacheDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new CASPartCachePage());
    public void OpenCompositorCacheClearDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new CompositorCacheClearPage());
    public void OpenScriptCacheDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new ScriptCachePage());
    public void OpenSimCompositorCacheDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new SimCompositorCachePage());
    public void OpenSocialCacheDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new SocialCachePage());
    public void OpenWorldCachesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new WorldCachesPage());
    public void OpenThumbnailsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new ThumbnailsPage());
    public void OpenFeaturedItemsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new FeaturedItemsPages());
    public void OpenAllXmlDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new AllXmlPage());
    public void OpenDCCacheDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new DCCachePage());

    /// <summary>
    /// Opens a modal window with a prohibited list of resources
    /// </summary>
    /// <param name="gameSave">Selected game save</param>
    public void OpenProhibitedList(GameSave gameSave)
    {
        ProhibitedListWindow window = new(gameSave);
        window.Show(App.MainWindow);
    }
    
    /// <summary>
    /// Opens a modal window that allows you to merge the old prohibited list to the current one
    /// </summary>
    /// <param name="gameSave">Selected game save</param>
    public async void OpenOldProhibitedLists(GameSave gameSave)
    {
        OldProhibitedListsWindow window = new(gameSave, GameSaves.Select(x => x.Name));

        if (await window.Show<bool>(App.MainWindow))
            ShowNotification("Successful", "MergedOldList", NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    /// <summary>
    /// Opens a modal window that allows you to move the save to another folder
    /// </summary>
    public async void OpenGameSavesTransferWindow()
    {
        GameSavesTransferWindow window = new();

        if (!await window.Show<bool>(App.MainWindow))
            return;

        await LoadSaves();
        ShowNotification("Successful", "YourSavesHaveBeenMoved", NotificationType.Success, TimeSpan.FromSeconds(3));
    }
    
    /// <summary>
    /// Allows you to select the path to the directory where saves will be backed up
    /// </summary>
    public async void SelectBackupPath()
    {
        string? path = await StorageProvider.SelectDirectory(App.MainWindow);

        if (path is not null)
            RegulSettings.Instance.PathToBackup = path;
    }

    /// <summary>
    /// Creates backups of saves to the selected directory
    /// </summary>
    public async void CreateBackups()
    {
        InCreatingBackupProcess = true;

        await Task.Run(() =>
        {
            foreach (GameSave gameSave in SelectedGameSaves.Where(_ => !string.IsNullOrWhiteSpace(RegulSettings.Instance.PathToSaves)))
                DirectoryManager.Copy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToBackup, gameSave.Name + ".sims3"), true);
        });

        InCreatingBackupProcess = false;
        
        ShowNotification("Successful", "BackupCreated", NotificationType.Success, TimeSpan.FromSeconds(3));
    }

    /// <summary>
    /// Checks the number of saves loaded in the program
    /// </summary>
    /// <returns>Whether the number of saves in the program exceeds the number set in the settings</returns>
    private bool CheckNumberOfSaves()
    {
        if (!RegulSettings.Instance.ShowWarningAboutLargeNumberOfSaves) return false;
        return GameSaves.Count >= RegulSettings.Instance.NumberOfSavesWhenWarningIsDisplayed;
    }

    /// <summary>
    /// Starts the process of moving saves to another directory
    /// </summary>
    /// <param name="gameSave">Game save</param>
    public async void StartMovingSave(GameSave gameSave)
    {
        IsMovingSaves = true;
        try
        {
            await Task.Run(() =>
            {
                MoveSaveToSpareFolder(gameSave);
            });
        }
        catch
        {
            IsMovingSaves = false;
            ShowNotification("Error", "MovingIsNotPossible", NotificationType.Error, TimeSpan.FromSeconds(5));
            return;
        }

        IsMovingSaves = false;
        
        await LoadSaves();

        ShowNotification("Successful", "YourSaveHasBeenMoved", NotificationType.Success);
    }
    
    /// <summary>
    /// Asynchronously moves the selected game saves to a spare folder. If there is any error during the operation,
    /// an error notification is shown to the user. If the operation is successful, a success notification is shown.
    /// </summary>
    /// <remarks>
    /// The method sets the flag <see cref="IsLoadingSaves"/> to true at the start of the operation and resets it after the operation is completed or an error occurs.
    /// In case of an error, the method also shows an error notification with a duration of 5 seconds.
    /// The moving operation is performed in a separate task to prevent UI thread blocking. 
    /// After moving all the game saves, it reloads the saves and shows a success notification.
    /// </remarks>
    /// <exception cref="Exception">
    /// Throws an exception if the moving operation fails.
    /// </exception>
    public async void MoveSavesToSpareFolder()
    {
        IsMovingSaves = true;
        try
        {
            await Task.Run(() =>
            {
                foreach (GameSave gameSave in SelectedGameSaves)
                    MoveSaveToSpareFolder(gameSave);
            });
        }
        catch
        {
            IsMovingSaves = false;
            ShowNotification("Error", "MovingIsNotPossible", NotificationType.Error, TimeSpan.FromSeconds(5));
            return;
        }
        IsMovingSaves = false;
        
        await LoadSaves();
        
        ShowNotification("Successful", "YourSavesHaveBeenMoved", NotificationType.Success);
    }

    /// <summary>
    /// Starts the process of clearing the cache
    /// </summary>
    public async void StartCleaningCache()
    {
        List<string> deletedFiles = new();

        InCleaningCacheProcess = true;
        await Task.Run(() => ClearCache(deletedFiles));
        InCleaningCacheProcess = false;

        if (deletedFiles.Count == 0)
        {
            ShowNotification(App.GetString("Information"), App.GetString("CacheHasAlreadyBeenCleared"), NotificationType.Information);
            return;
        }
        
        ClearedCacheNotification notification = new()
        {
            DeletedFiles = deletedFiles,
            Type = NotificationType.Success
        };

        NotificationManager.Show(notification);
    }

    /// <summary>
    /// Starts the process of clearing saves
    /// </summary>
    public void StartCleaning()
    {
        InCleaningProcess = true;
        
        List<CleaningResult> cleaningResults = new();
        List<string> deletedFiles = new();

        LoadingWindow loadingWindow = new()
        {
            TextBlock = { Text = App.GetString("Preparation") }
        };

        loadingWindow.Show(App.MainWindow);

        Thread thread = new(() =>
        {
            try
            {
                Clean(loadingWindow, cleaningResults);
            }
            catch (UnauthorizedAccessException)
            {
                _synchronizationContext.Send(_ =>
                {
                    loadingWindow.Close();
                    
                    MessageBox.Show(App.MainWindow, App.GetString("UnableToCleanSaves"), App.GetString("UnableToCleanSavesDescription") + $"\n\n{_currentCleaningSaveName}");
                    InCleaningProcess = false;
                }, null);
                
                return;
            }
            
            if (RegulSettings.Instance.ClearCache)
            {
                UpdateLoadingWindow(loadingWindow, 0, true, 100, "CleaningCache");
                
                ClearCache(deletedFiles);
            }
            
            _synchronizationContext.Send(_ =>
            {
                loadingWindow.Close();

                string results = BuildClearingResults(cleaningResults, deletedFiles);

                if (App.MainWindow.WindowState == WindowState.Minimized)
                    App.MainWindow.WindowState = WindowState.Normal;

                string title = cleaningResults.Count > 1 ? App.GetString("SavesCleared") : App.GetString("SaveCleared");

                MessageBox.Show(App.MainWindow, title, App.GetString("ResultsBelow"), additionalText: results);

                InCleaningProcess = false;
            }, null);
        })
        {
            Priority = ThreadPriority.Highest
        };
        
        thread.Start();
    }
    
    /// <summary>
    /// Switches the settings for clearing saves
    /// </summary>
    /// <param name="value">On or off</param>
    private void ToggleClearOptions(bool value)
    {
        RegulSettings.Instance.RemovePortraitsSims = value;
        RegulSettings.Instance.RemoveLotThumbnails = value;
        RegulSettings.Instance.RemovePhotos = value;
        RegulSettings.Instance.RemoveFamilyPortraits = value;
        RegulSettings.Instance.RemoveGeneratedImages = value;

        if (value)
            return;

        RegulSettings.Instance.RemoveTextures = false;
        RegulSettings.Instance.RemoveOtherTypes = false;
    }
    
    /// <summary>
    /// Switches the cache clearing settings
    /// </summary>
    /// <param name="value">On or off</param>
    private void ToggleCacheOptions(bool value)
    {
        RegulSettings.Instance.CasPartCacheClear = value;
        RegulSettings.Instance.CompositorCacheClear = value;
        RegulSettings.Instance.ScriptCacheClear = value;
        RegulSettings.Instance.SimCompositorCacheClear = value;
        RegulSettings.Instance.SocialCacheClear = value;

#if !OSX
        RegulSettings.Instance.WorldCachesClear = value;
#endif
        
        RegulSettings.Instance.IgaCacheClear = value;
        RegulSettings.Instance.ThumbnailsClear = value;
        RegulSettings.Instance.FeaturedItemsClear = value;
        RegulSettings.Instance.AllXmlClear = value;
        RegulSettings.Instance.DccClear = value;
        RegulSettings.Instance.DownloadedSimsClear = value;
        RegulSettings.Instance.LogClear = value;
        RegulSettings.Instance.DcBackupPackagesClear = value;
        RegulSettings.Instance.MissingDepsClear = value;
    }
    
    /// <summary>
    /// Moves a GameSave object's associated directory to a 'spare' folder.
    /// </summary>
    /// <param name="gameSave">The GameSave object whose folder is to be moved.</param>
    private void MoveSaveToSpareFolder(GameSave gameSave)
    {
        if (!Directory.Exists(RegulSettings.Instance.PathToFolderWithOldSaves))
            Directory.CreateDirectory(RegulSettings.Instance.PathToFolderWithOldSaves);
        
        DirectoryManager.Copy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToFolderWithOldSaves, gameSave.Name + ".sims3"), true);
        Directory.Delete(gameSave.Directory, true);
    }

    private void OpenCleaningOptionDescriptionWindow(ContentControl userControl)
    {
        CleaningOptionDescriptionWindow window = new()
        {
            Content = userControl
        };

        window.Show(App.MainWindow);
    }

    /// <summary>
    /// Creates a list of results after clearing saves
    /// </summary>
    /// <param name="cleaningResults"></param>
    /// <param name="deletedFiles"></param>
    /// <returns></returns>
    private static string BuildClearingResults(List<CleaningResult> cleaningResults, List<string> deletedFiles)
    {
        StringBuilder stringBuilder = new();

        foreach (CleaningResult cleaningResult in cleaningResults)
        {
            stringBuilder.Append($"{App.GetString("SaveName")}: {cleaningResult.Save}");
            stringBuilder.Append($"\n{App.GetString("TimePassed")}: {cleaningResult.TotalSecond} {App.GetString("Sec")}");
            
            stringBuilder.Append($"\n\n{App.GetString("OldSize")}: {cleaningResult.OldSize} MB");
            stringBuilder.Append($"\n{App.GetString("NewSize")}: {cleaningResult.NewSize} MB");
            stringBuilder.Append($"\n{App.GetString("Percent")}: {(cleaningResult.OldSize - cleaningResult.NewSize) / cleaningResult.OldSize:P}");

            if (cleaningResults.IndexOf(cleaningResult) + 1 < cleaningResults.Count)
                stringBuilder.Append("\n-----------------------\n");
        }

        if (deletedFiles.Count <= 0)
            return stringBuilder.ToString();

        stringBuilder.Append("\n-----------------------");

        foreach (string deletedFile in deletedFiles)
            stringBuilder.Append($"\n{deletedFile}");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Shows a notification with the specified title, description, and notification type.
    /// </summary>
    /// <param name="title">The title of the notification, which will be localized using the <see cref="App.GetString"/> function.</param>
    /// <param name="description">The description of the notification, which will be localized using the <see cref="App.GetString"/> function.</param>
    /// <param name="notificationType">The type of the notification. This parameter controls the appearance and behavior of the notification.</param>
    /// <param name="timeSpan">Optional parameter. The duration for which the notification will be shown. If not specified, the default duration will be used.</param>
    private void ShowNotification(string title, string description, NotificationType notificationType, TimeSpan timeSpan = default)
    {
        NotificationManager.Show(new Notification(App.GetString(title), App.GetString(description),
            notificationType, timeSpan));
    }

    /// <summary>
    /// Updates the progress bar and text block in the given <see cref="LoadingWindow"/>.
    /// </summary>
    /// <param name="loadingWindow">The <see cref="LoadingWindow"/> to update.</param>
    /// <param name="value">The current value to set for the progress bar.</param>
    /// <param name="isIndeterminate">Whether the progress of the task is indeterminate.</param>
    /// <param name="maximum">The maximum value the progress bar can reach.</param>
    /// <param name="text">The key for the text to display in the text block.</param>
    /// <remarks>
    /// This method runs on the UI thread.
    /// </remarks>
    private void UpdateLoadingWindow(LoadingWindow loadingWindow, double value, bool isIndeterminate, double maximum, string text)
    {
        _synchronizationContext.Send(_ =>
        {
            loadingWindow.ProgressBar.Value = value;
            loadingWindow.ProgressBar.IsIndeterminate = isIndeterminate;
            loadingWindow.ProgressBar.Maximum = maximum;
            loadingWindow.TextBlock.Text = App.GetString(text);
        }, "");
    }

    /// <summary>
    /// Compresses resource entry if it's not compressed and its file size differs from its memory size.
    /// </summary>
    /// <param name="entry">The resource index entry to process.</param>
    /// <remarks>
    /// Marks the resource as compressed by setting the 'Compressed' property to 'ushort.MaxValue'.
    /// This method is purely used for data status correction as it doesn't perform any actual data compression.
    /// </remarks>
    private static void CompressResource(ResourceIndexEntry entry)
    {
        if (entry.Compressed == 0 && entry.FileSize != entry.MemSize)
            entry.Compressed = ushort.MaxValue;
    }

    /// <summary>
    /// Deletes a specific resource of the game data based on resource type and resource group, if enabled. 
    /// </summary>
    /// <param name="entry">The resource entry to delete.</param>
    /// <param name="gameDataType">The data type of the game that contains particular resource types and groups.</param>
    /// <param name="enableDelete">A flag indicating whether the delete operation is enabled or not. If not enabled, the function returns false immediately.</param>
    /// <returns>
    /// A boolean value indicating whether the deletion is successful or not. If the delete operation isn't enabled, or the resource cannot be found, the deletion is considered unsuccessful and the function returns false.
    /// </returns>
    private static bool DeleteResourceByType(ResourceIndexEntry entry, GameDataType gameDataType, bool enableDelete)
    {
        if (!enableDelete) return false;
        
        bool isDeleted = Array.Exists(gameDataType.ResourceTypes, x => x == entry.ResourceType);

        if (gameDataType.ResourceGroups is not null && isDeleted)
            isDeleted = Array.Exists(gameDataType.ResourceGroups, x => x == entry.ResourceGroup);

        if (isDeleted)
            entry.IsDeleted = true;
        return isDeleted;
    }

    /// <summary>
    /// Deletes a resource index entry if it meets specific conditions.
    /// </summary>
    /// <param name="entry">The <see cref="ResourceIndexEntry"/> object to be checked and potentially deleted.</param>
    /// <returns>
    /// Returns true if the resource index entry was deleted based on the conditions,
    /// and false if the entry was not deleted.
    /// </returns>
    /// <remarks>
    /// This method checks the following conditions:
    /// <list type="number">
    /// <item><description>Whether the <see cref="RegulSettings.RemoveOtherTypes"/> property is true.</description></item>
    /// <item><description>Whether the <see cref="ResourceIndexEntry.MemSize"/> of the resource index entry is equal to <b>0x2AB38</b></description></item>
    /// </list>
    /// If both conditions are true, the 'IsDeleted' property on the resource index entry is set to true,
    /// meaning the entry is considered deleted.
    /// </remarks>
    private static bool DeleteOtherResources(ResourceIndexEntry entry)
    {
        if (!RegulSettings.Instance.RemoveOtherTypes || entry.MemSize != 0x2AB38)
            return false;

        entry.IsDeleted = true;
        return true;
    }

    private static bool CheckResourceInProhibitedList(ResourceIndexEntry entry, GameSaveResource? resource)
    {
        return resource is not null && resource.ProhibitedResources.Any(IsProhibited);

        bool IsProhibited(ProhibitedResource prohibitedResource) =>
            prohibitedResource.Type == entry.ResourceType
            && prohibitedResource.Instance == entry.Instance
            && prohibitedResource.Group == entry.ResourceGroup;
    }

    /// <summary>
    /// Cleans up selected game saves by processing packages and saves, potentially creating a backup.
    /// </summary>
    /// <param name="loadingWindow">An instance of a <see cref="LoadingWindow"/> object this method is expected to interact with.</param>
    /// <param name="cleaningResults">A list of <see cref="CleaningResult"/> objects to which the results of the cleanup will be added.</param>
    /// <notes>
    /// This method will iterate over the selected game saves, perform processes on them, measure the time spent for each save, and
    /// compile a <see cref="CleaningResult"/> for each. Each resultant <see cref="CleaningResult"/>, containing old and new sizes of the game save as well as the time spent, 
    /// is then added to the provided cleaningResults list. If the <see cref="RegulSettings.CreateBackup"/> option is enabled, a backup will be created.
    /// </notes>
    private void Clean(LoadingWindow loadingWindow, List<CleaningResult> cleaningResults)
    {
        const float kilobyte = 1.0f / 1048576;

        foreach (GameSave gameSave in SelectedGameSaves)
        {
            _currentCleaningSaveName = gameSave.Name;
            
            GameSaveResource? resource = RegulSettings.Instance.GameSaveResources.FirstOrDefault(x => x.Id == gameSave.Name);
            long oldGameSaveSize = CalculateDirectorySize(gameSave.Directory);

            if (RegulSettings.Instance.CreateBackup)
                CreateGameSaveBackup(loadingWindow, gameSave);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            ProcessPackages(loadingWindow, gameSave);
            ProcessTravelDbPackage(loadingWindow, gameSave, resource);
            ProcessSaves(loadingWindow, gameSave, resource);
            
            stopwatch.Stop();

            long newGameSaveSize = CalculateDirectorySize(gameSave.Directory);

            CleaningResult cleaningResult = new(
                oldGameSaveSize * kilobyte,
                newGameSaveSize * kilobyte,
                stopwatch.Elapsed.TotalSeconds,
                gameSave.Name);
            
            cleaningResults.Add(cleaningResult);
        }
    }

    private long CalculateDirectorySize(string directoryPath)
    {
        return Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                        .Sum(path => new FileInfo(path).Length);
    }

    private void CreateGameSaveBackup(LoadingWindow loadingWindow, GameSave gameSave)
    {
        UpdateLoadingWindow(loadingWindow, 0, true, 100, $"{gameSave.Name}\n{App.GetString("CreatingABackup")}");
        DirectoryManager.Copy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToBackup, gameSave.Name + ".sims3"), true);
    }

    private void ProcessPackages(LoadingWindow loadingWindow, GameSave gameSave)
    {
        foreach (string path in Directory.EnumerateFiles(gameSave.Directory, "*.package", SearchOption.AllDirectories))
        {
            if (path == Path.Combine(gameSave.Directory, "TravelDB.package")) continue;
                
            Package package = Package.OpenPackage(path, true);
                
            UpdateLoadingWindow(loadingWindow, 0, false, package.GetResourceList.Count, $"{gameSave.Name}\n{App.GetString("ProcessingFile")}: {Path.GetFileName(path)}");

            Parallel.ForEach(package.GetResourceList, entry =>
            {
                _synchronizationContext.Send(_ => { loadingWindow.ProgressBar.Value += 1; }, "");
                    
                if (DeleteOtherResources(entry)) return;
                    
                CompressResource(entry);
            });
                
            package.SavePackage();
            Package.ClosePackage(package);
        }
    }

    private void ProcessTravelDbPackage(LoadingWindow loadingWindow, GameSave gameSave, GameSaveResource? resource)
    {
        string travelDB = Path.Combine(gameSave.Directory, "TravelDB.package");
        
        if ((!File.Exists(travelDB) || !RegulSettings.Instance.RemoveGeneratedImages) && !RegulSettings.Instance.RemovePhotos)
            return;
                
        Package travelDBPackage = Package.OpenPackage(travelDB, true);
                
        UpdateLoadingWindow(loadingWindow, 0, false, travelDBPackage.GetResourceList.Count, $"{gameSave.Name}\n{App.GetString("ProcessingFile")}: {Path.GetFileName(travelDB)}");
                
        Parallel.ForEach(travelDBPackage.GetResourceList, entry =>
        {
            _synchronizationContext.Send(_ => { loadingWindow.ProgressBar.Value += 1; }, "");
                    
            if (CheckResourceInProhibitedList(entry, resource))
            {
                CompressResource(entry);
                return;
            }

            if (DeleteOtherResources(entry)) return;

            if (DeleteResourceByType(entry, GameDataTypes.GeneratedImages, RegulSettings.Instance.RemoveGeneratedImages)) return;

            if (DeleteResourceByType(entry, GameDataTypes.Photos, RegulSettings.Instance.RemovePhotos)) return;

            CompressResource(entry);
        });
                
        travelDBPackage.SavePackage();
        Package.ClosePackage(travelDBPackage);
    }

    private void ProcessSaves(LoadingWindow loadingWindow, GameSave gameSave, GameSaveResource? resource)
    {
        foreach (string path in Directory.EnumerateFiles(gameSave.Directory, "*.nhd", SearchOption.AllDirectories))
        {
            Package nhd = Package.OpenPackage(path, true);
                
            UpdateLoadingWindow(loadingWindow, 0, false, nhd.GetResourceList.Count, $"{gameSave.Name}\n{App.GetString("ProcessingFile")}: {Path.GetFileName(path)}");

            Parallel.ForEach(nhd.GetResourceList, entry =>
            {
                _synchronizationContext.Send(_ => { loadingWindow.ProgressBar.Value += 1; }, "");
                    
                if (CheckResourceInProhibitedList(entry, resource))
                {
                    CompressResource(entry);
                    return;
                }
                    
                if (DeleteOtherResources(entry)) return;
                    
                if (DeleteResourceByType(entry, GameDataTypes.SimPortraits, RegulSettings.Instance.RemovePortraitsSims)) return;
                    
                if (gameSave.ImageInstance != entry.Instance && DeleteResourceByType(entry, GameDataTypes.FamilyPortraits, RegulSettings.Instance.RemoveFamilyPortraits)) return;
                    
                if (DeleteResourceByType(entry, GameDataTypes.GeneratedImages, RegulSettings.Instance.RemoveGeneratedImages)) return;
                    
                if (DeleteResourceByType(entry, GameDataTypes.Photos, RegulSettings.Instance.RemovePhotos)) return;
                    
                if (DeleteResourceByType(entry, GameDataTypes.Textures, RegulSettings.Instance.RemoveTextures)) return;
                    
                if (DeleteResourceByType(entry, GameDataTypes.LotThumbnails, RegulSettings.Instance.RemoveLotThumbnails)) return;
                    
                CompressResource(entry);
            });
                
            nhd.SavePackage();
            Package.ClosePackage(nhd);
        }
    }

    /// <summary>
    /// Clears specified caches depending on the boolean variables set in the <see cref="RegulSettings"/> instance.
    /// </summary>
    /// <param name="deletedFiles">An optional collection to store the paths of the deleted files. If this parameter is null, the method will not store the paths of the deleted files.</param>
    /// <remarks>
    /// This method handles the deletion of various types of cache data used in the system. It uses the settings from the <see cref="RegulSettings"/> singleton instance to determine which cache files to delete.
    /// The method uses the DeleteCache and DeleteFile methods for the actual deletion process. These methods should handle the deletion process.
    /// Some cache files have conditional deletion based on the platform (OSX). Some cache data files are deleted in parallel for a better performance.
    /// </remarks>
    private void ClearCache(ICollection<string>? deletedFiles = null)
    {
        DeleteCache(RegulSettings.Instance.CasPartCacheClear, "CASPartCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.CompositorCacheClear, "compositorCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.ScriptCacheClear, "scriptCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.SimCompositorCacheClear, "simCompositorCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.SocialCacheClear, "socialCache.package", deletedFiles);

#if !OSX
        DeleteCache(RegulSettings.Instance.WorldCachesClear, "WorldCaches", deletedFiles);
#endif
        
        DeleteCache(RegulSettings.Instance.IgaCacheClear, "IGACache", deletedFiles);
        DeleteCache(RegulSettings.Instance.ThumbnailsClear, "Thumbnails", deletedFiles);
        
        DeleteCache(RegulSettings.Instance.FeaturedItemsClear, "FeaturedItems", deletedFiles);
        
        DeleteCache(RegulSettings.Instance.AllXmlClear, "", deletedFiles, "ScriptError_*.xml");
        DeleteCache(RegulSettings.Instance.DccClear, Path.Combine("DCCache", "dcc.ent"), deletedFiles);

        if (RegulSettings.Instance.LogClear)
        {
            DeleteCache(true, "DeviceConfig.log", deletedFiles);
            DeleteCache(true, "Sims3LauncherLogFile.log", deletedFiles);
            DeleteCache(true, "Sims3Logs.xml", deletedFiles);
        }

        string dcBackup = Path.Combine(RegulSettings.Instance.PathToTheSims3Document, "DCBackup");
        if (RegulSettings.Instance.DcBackupPackagesClear && Directory.Exists(dcBackup))
        {
            Parallel.ForEach(Directory.EnumerateFiles(dcBackup, "*.package", SearchOption.TopDirectoryOnly), path =>
                {
                    if (Path.GetFileName(path) != "ccmerged.package")
                        DeleteFile(path, deletedFiles);
                });
        }

        DeleteCache(RegulSettings.Instance.MissingDepsClear, Path.Combine("DCCache", "missingdeps.idx"), deletedFiles);
        DeleteCache(RegulSettings.Instance.DccClear, Path.Combine("SavedSims", "DownloadedSims"), deletedFiles);
    }

    /// <summary>
    /// Deletes files in specified cache directory or a specific cache file based on provided conditions.
    /// If the provided path points to a directory, every file in that directory that matches the search pattern will be deleted.
    /// </summary>
    /// <param name="clean">Determines whether the operation should be executed or not. If this parameter is false, the method will return immediately.</param>
    /// <param name="cache">Relative path to the cache directory or cache file from 'The Sims 3' documents directory.</param>
    /// <param name="deletedFiles">Optional collection for keeping track of the deleted files. If provided, the full path to every deleted file will be added to this collection.</param>
    /// <param name="searchPattern">Optional search pattern used when deleting files from a directory. Default value is "*", which matches any file.</param>
    /// <remarks>
    /// This method will attempt to delete all files even if one or more deletions fail. Exceptions that occur during deletion of individual files are silently caught and do not prevent the method from continuing.
    /// </remarks>
    private void DeleteCache(bool clean, string cache, ICollection<string>? deletedFiles, string searchPattern = "*")
    {
        if (!clean) return;
        
        string pathToCache = Path.Combine(RegulSettings.Instance.PathToTheSims3Document, cache);

        if (!Directory.Exists(pathToCache) && !File.Exists(pathToCache)) return;

        FileAttributes attributes = File.GetAttributes(pathToCache);

        try
        {
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                Parallel.ForEach(Directory.EnumerateFiles(pathToCache, searchPattern), file =>
                {
                    DeleteFile(file, deletedFiles);
                });
            }
            else DeleteFile(pathToCache, deletedFiles);
        }
        catch { }
    }
    
    /// <summary>
    /// Deletes the file at the specified path
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="files">Deleted files list</param>
    private void DeleteFile(string path, ICollection<string>? files = null)
    {
        File.Delete(path);
        files?.Add(path);
    }
}