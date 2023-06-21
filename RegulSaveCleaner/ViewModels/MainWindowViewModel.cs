using System.Diagnostics;
using System.Text;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using PleasantUI;
using PleasantUI.Windows;
using RegulSaveCleaner.S3PI.Interfaces;
using RegulSaveCleaner.S3PI.Package;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.Views.InformationPages;
using RegulSaveCleaner.Views.Windows;

namespace RegulSaveCleaner.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current!;
    
    private bool _inCreatingBackupProcess;
    private bool _inCleaningCacheProcess;
    private bool _isLoadingSaves;
    private bool _canBeCleaningCache;
    private bool _foundSaveFolder;

    private bool _sortByAlphabet;
    private bool _sortByDate = true;
    
    private AvaloniaList<GameSave> _selectedGameSaves = new();
    private IManagedNotificationManager _notificationManager = null!;
    
    public AvaloniaList<GameSave> GameSaves { get; set; } = new();

    public bool IsMacOs
    {
        get
        {
#if NET461
            return false;
#else
            return OperatingSystem.IsMacOS();
#endif
        }
    }
    
    public IManagedNotificationManager NotificationManager
    {
        get => _notificationManager;
        set => RaiseAndSet(ref _notificationManager, value);
    }

    public AvaloniaList<GameSave> SelectedGameSaves
    {
        get => _selectedGameSaves;
        set => RaiseAndSet(ref _selectedGameSaves, value);
    }
    
    public bool InCreatingBackupProcess
    {
        get => _inCreatingBackupProcess;
        set => RaiseAndSet(ref _inCreatingBackupProcess, value);
    }

    public bool InCleaningCacheProcess
    {
        get => _inCleaningCacheProcess;
        set => RaiseAndSet(ref _inCleaningCacheProcess, value);
    }
    
    public bool IsLoadingSaves
    {
        get => _isLoadingSaves;
        set => RaiseAndSet(ref _isLoadingSaves, value);
    }

    public bool CanBeCleaningCache
    {
        get => _canBeCleaningCache;
        set => RaiseAndSet(ref _canBeCleaningCache, value);
    }
    
    public bool FoundSaveFolder
    {
        get => _foundSaveFolder;
        set => RaiseAndSet(ref _foundSaveFolder, value);
    }

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

    public MainWindowViewModel() { }

    public MainWindowViewModel(TopLevel host)
    {
        _notificationManager = new WindowNotificationManager(host)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
    }

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

    public async void LoadingSaves()
    {
        FoundSaveFolder = false;
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
                    string? path = await StorageProvider.SelectFolder(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

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
                        string? path = await StorageProvider.SelectFolder(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

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

            FoundSaveFolder = true;

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

                            GameSave gameSave = new(directory);

                            if (file.IndexOf(gameSave.Location, StringComparison.Ordinal) != -1)
                            {
                                if (gameSaveData.FamilyIcon is null)
                                {
                                    _synchronizationContext.Send(_ =>
                                    {
                                        gameSave.ImageOfFamily = App.GetResource<DrawingImage>("UnknownIcon");
                                    }, "");
                                }
                                else gameSave.ImageOfFamily = gameSaveData.FamilyIcon;

                                gameSave.WorldName = gameSaveData.WorldName;
                                gameSave.Description = gameSaveData.Description;
                                gameSave.ImageInstance = gameSaveData.ImgInstance;
                                gameSave.LastSaveTime = gameSaveData.LastSaveTime;
                                
                                _synchronizationContext.Send(_ =>
                                {
                                    GameSaves.Add(gameSave);
                                    if (saves.Any(save => gameSave.Name == save))
                                        SelectedGameSaves.Add(gameSave);
                                }, "");
                            }
                        }
                    });
                }
                catch (Exception e)
                {
                    await MessageBox.Show(App.MainWindow, App.GetString("AnErrorHasOccured"), string.Empty, additionalText: e.ToString());
                }
            }
            
            break;
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

    public void SelectAllClearOptions()
    {
        RegulSettings.Instance.RemovePortraitsSims = true;
        RegulSettings.Instance.RemoveLotThumbnails = true;
        RegulSettings.Instance.RemovePhotos = true;
        RegulSettings.Instance.RemoveFamilyPortraits = true;
        RegulSettings.Instance.RemoveGeneratedImages = true;
    }
    
    public void CancelAllClearOptions()
    {
        RegulSettings.Instance.RemovePortraitsSims = false;
        RegulSettings.Instance.RemoveLotThumbnails = false;
        RegulSettings.Instance.RemovePhotos = false;
        RegulSettings.Instance.RemoveFamilyPortraits = false;
        RegulSettings.Instance.RemoveGeneratedImages = false;
        RegulSettings.Instance.RemoveTextures = false;
        RegulSettings.Instance.RemoveOtherTypes = false;
    }
    
    public void SelectAllCacheOptions()
    {
        RegulSettings.Instance.CasPartCacheClear = true;
        RegulSettings.Instance.CompositorCacheClear = true;
        RegulSettings.Instance.ScriptCacheClear = true;
        RegulSettings.Instance.SimCompositorCacheClear = true;
        RegulSettings.Instance.SocialCacheClear = true;

#if !NET461
        if (!OperatingSystem.IsMacOS())
#endif
            RegulSettings.Instance.WorldCachesClear = true;

        RegulSettings.Instance.IgaCacheClear = true;
        RegulSettings.Instance.ThumbnailsClear = true;
        RegulSettings.Instance.FeaturedItemsClear = true;
        RegulSettings.Instance.AllXmlClear = true;
        RegulSettings.Instance.DccClear = true;
        RegulSettings.Instance.DownloadedSimsClear = true;
        RegulSettings.Instance.LogClear = true;
        RegulSettings.Instance.DcBackupPackagesClear = true;
        RegulSettings.Instance.MissingDepsClear = true;
    }
    
    public void CancelAllCacheOptions()
    {
        RegulSettings.Instance.CasPartCacheClear = false;
        RegulSettings.Instance.CompositorCacheClear = false;
        RegulSettings.Instance.ScriptCacheClear = false;
        RegulSettings.Instance.SimCompositorCacheClear = false;
        RegulSettings.Instance.SocialCacheClear = false;
        RegulSettings.Instance.WorldCachesClear = false;
        RegulSettings.Instance.IgaCacheClear = false;
        RegulSettings.Instance.ThumbnailsClear = false;
        RegulSettings.Instance.FeaturedItemsClear = false;
        RegulSettings.Instance.AllXmlClear = false;
        RegulSettings.Instance.DccClear = false;
        RegulSettings.Instance.DownloadedSimsClear = false;
        RegulSettings.Instance.LogClear = false;
        RegulSettings.Instance.DcBackupPackagesClear = false;
        RegulSettings.Instance.MissingDepsClear = false;
    }

    public void OpenRemoveFamilyPortraitsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveFamilyPortraitsPage());
    public void OpenRemovePortraitsSimsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemovePortraitsOfSimsPage());
    public void OpenRemoveLotThumbnailsDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveLotThumbnailsPage());
    public void OpenRemovePhotosDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemovePhotosPage());
    public void OpenRemoveGeneratedImagesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveGeneratedImagesPage());
    public void OpenRemoveTexturesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveTexturesPage());
    public void OpenRemoveOtherTypesDescriptionWindow() => OpenCleaningOptionDescriptionWindow(new RemoveOtherTypesPage());

    public async void OpenProhibitedList(GameSave gameSave)
    {
        ProhibitedListWindow window = new(gameSave);
        await window.Show(App.MainWindow);
        await Task.Delay(1000);
        ClearGc();
    }
    
    public async void OpenOldProhibitedLists(GameSave gameSave)
    {
        OldProhibitedListsWindow window = new(gameSave, GameSaves.Select(x => x.Name));
        bool result = await window.Show<bool>(App.MainWindow);

        if (result)
        {
            NotificationManager.Show(new Notification(App.GetString("Successful"),
                App.GetString("MergedOldList"),
                NotificationType.Success,
                TimeSpan.FromSeconds(3)));
        }
    }
    
    public async void SelectBackupPath()
    {
        string? path = await StorageProvider.SelectFolder(App.MainWindow);

        if (path is not null)
            RegulSettings.Instance.PathToBackup = path;
    }

    public async void CreateBackups()
    {
        InCreatingBackupProcess = true;

        await Task.Run(() =>
        {
            foreach (GameSave gameSave in SelectedGameSaves.Where(_ => !string.IsNullOrWhiteSpace(RegulSettings.Instance.PathToSaves)))
                DirectoryCopy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToBackup, gameSave.Name + ".sims3"), true);
        });

        InCreatingBackupProcess = false;
        
        ShowNotification("Successful", "BackupCreated", NotificationType.Success);
    }

    public async void StartCleaningCache()
    {
        InCleaningCacheProcess = true;
        await Task.Run(() => ClearCache());
        InCleaningCacheProcess = false;
        
        ShowNotification("Successful", "CacheCleared", NotificationType.Success);
    }

    public void StartCleaning()
    {
        List<CleaningResult> cleaningResults = new();
        List<string> deletedFiles = new();

        LoadingWindow loadingWindow = new()
        {
            TextBlock = { Text = App.GetString("Preparation") }
        };

        loadingWindow.Show(App.MainWindow);

        Thread thread = new(() =>
        {
            Clean(loadingWindow, cleaningResults);

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
            }, "");
        })
        {
            Priority = ThreadPriority.Highest
        };
        
        thread.Start();
    }

    private void OpenCleaningOptionDescriptionWindow(ContentControl userControl)
    {
        CleaningOptionDescriptionWindow window = new()
        {
            Content = userControl
        };

        window.Show(App.MainWindow);
    }

    private string BuildClearingResults(List<CleaningResult> cleaningResults, List<string> deletedFiles)
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

        if (deletedFiles.Count > 0)
        {
            stringBuilder.Append("\n-----------------------");

            foreach (string deletedFile in deletedFiles)
                stringBuilder.Append($"\n{deletedFile}");
        }

        return stringBuilder.ToString();
    }

    private void ShowNotification(string title, string description, NotificationType notificationType, TimeSpan timeSpan = default)
    {
        NotificationManager.Show(new Notification(App.GetString(title), App.GetString(description),
            notificationType, timeSpan));
    }

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

    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new(sourceDirName);

        if (!dir.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDirName);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subDir.Name);
                DirectoryCopy(subDir.FullName, tempPath, true);
            }
        }
    }

    private void CompressResource(IResourceIndexEntry entry)
    {
        if (entry.Compressed == 0 && entry.Filesize != entry.Memsize)
            entry.Compressed = ushort.MaxValue;
    }

    private bool DeleteResourceByType(IResourceIndexEntry entry, GameDataType gameDataType, bool enableDelete)
    {
        if (!enableDelete) return false;
        
        bool isDeleted = gameDataType.ResourceTypes.Any(x => x == entry.ResourceType);

        if (gameDataType.ResourceGroups is not null && isDeleted)
            isDeleted = gameDataType.ResourceGroups.Any(x => x == entry.ResourceGroup);

        if (isDeleted)
            entry.IsDeleted = true;
        return isDeleted;
    }

    private bool DeleteOtherResources(IResourceIndexEntry entry)
    {
        if (RegulSettings.Instance.RemoveOtherTypes && entry.Memsize == 0x2AB38)
        {
            entry.IsDeleted = true;
            return true;
        }

        return false;
    }

    private bool CheckResourceInProhibitedList(IResourceIndexEntry entry, GameSaveResource? resource) => 
        resource is not null && resource.ProhibitedResources.Any(prohibitedResource => prohibitedResource.Type == entry.ResourceType && prohibitedResource.Instance == entry.Instance && prohibitedResource.Group == entry.ResourceGroup);

    private void Clean(LoadingWindow loadingWindow, List<CleaningResult> cleaningResults)
    {
        const float kilobyte = 1.0f / 1048576;

        foreach (GameSave gameSave in SelectedGameSaves)
        {
            GameSaveResource? resource = RegulSettings.Instance.GameSaveResources.FirstOrDefault(x => x.Id == gameSave.Name);
            long oldGameSaveSize = Directory.EnumerateFiles(gameSave.Directory, "*.*", SearchOption.AllDirectories)
                .Sum(path => new FileInfo(path).Length);

            if (RegulSettings.Instance.CreateBackup)
            {
                UpdateLoadingWindow(loadingWindow, 0, true, 100, $"{gameSave.Name}\n{App.GetString("CreatingABackup")}");
                
                DirectoryCopy(gameSave.Directory, Path.Combine(RegulSettings.Instance.PathToBackup, gameSave.Name + ".sims3"), true);
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Processing packages
            foreach (string path in Directory.EnumerateFiles(gameSave.Directory, "*.package", SearchOption.AllDirectories))
            {
                if (path == Path.Combine(gameSave.Directory, "TravelDB.package")) continue;
                
                IPackage package = Package.OpenPackage(path, true);
                
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
            
            // Processing TravelDB.package
            string travelDB = Path.Combine(gameSave.Directory, "TravelDB.package");
            if (File.Exists(travelDB) && RegulSettings.Instance.RemoveGeneratedImages || RegulSettings.Instance.RemovePhotos)
            {
                IPackage travelDBPackage = Package.OpenPackage(travelDB, true);
                
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
            
            // Processing saves
            foreach (string path in Directory.EnumerateFiles(gameSave.Directory, "*.nhd", SearchOption.AllDirectories))
            {
                IPackage nhd = Package.OpenPackage(path, true);
                
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
            
            stopwatch.Stop();

            long newGameSaveSize = Directory.EnumerateFiles(gameSave.Directory, "*.*", SearchOption.AllDirectories)
                .Sum(path => new FileInfo(path).Length);

            CleaningResult cleaningResult = new(
                oldGameSaveSize * kilobyte,
                newGameSaveSize * kilobyte,
                stopwatch.Elapsed.TotalSeconds,
                gameSave.Name);
            cleaningResults.Add(cleaningResult);
        }
    }

    private void ClearCache(ICollection<string>? deletedFiles = null)
    {
        DeleteCache(RegulSettings.Instance.CasPartCacheClear, "CASPartCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.CompositorCacheClear, "compositorCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.ScriptCacheClear, "scriptCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.SimCompositorCacheClear, "simCompositorCache.package", deletedFiles);
        DeleteCache(RegulSettings.Instance.SocialCacheClear, "socialCache.package", deletedFiles);

#if !NET461
        if (!OperatingSystem.IsMacOS())
#endif
            DeleteCache(RegulSettings.Instance.WorldCachesClear, "WorldCaches", deletedFiles);
        
        DeleteCache(RegulSettings.Instance.IgaCacheClear, "IGACache", deletedFiles);
        DeleteCache(RegulSettings.Instance.ThumbnailsClear, "Thumbnails", deletedFiles);

        try
        {
            DeleteCache(RegulSettings.Instance.FeaturedItemsClear, "FeaturedItems", deletedFiles);
        }
        catch { }
        
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

    private void DeleteCache(bool clean, string cache, ICollection<string>? deletedFiles, string searchPattern = "*")
    {
        if (!clean) return;
        
        string pathToCache = Path.Combine(RegulSettings.Instance.PathToTheSims3Document, cache);

        if (!Directory.Exists(pathToCache) && !File.Exists(pathToCache)) return;

        FileAttributes attributes = File.GetAttributes(pathToCache);

        if (attributes.HasFlag(FileAttributes.Directory))
        {
            Parallel.ForEach(Directory.EnumerateFiles(pathToCache, searchPattern), file =>
            {
                DeleteFile(file, deletedFiles);
            });
        }
        else DeleteFile(pathToCache, deletedFiles);
    }
    
    private void DeleteFile(string path, ICollection<string>? files = null)
    {
        File.Delete(path);
        files?.Add(path);
    }
    
    private void ClearGc()
    {
        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}