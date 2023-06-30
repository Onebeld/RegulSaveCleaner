using System.Globalization;
using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Media;
using PleasantUI;
using PleasantUI.Core;
using PleasantUI.Core.Constants;
using RegulSaveCleaner.Core.Constants;
using RegulSaveCleaner.Structures;

#if Linux
using PleasantUI.Core;
#endif

namespace RegulSaveCleaner.Core;

public class RegulSettings : ViewModelBase
{
    private AvaloniaList<GameSaveResource> _gameSaveResource = new();

    private string _language = null!;
    private string _fontName = null!;

    private bool _showWarningAboutLargeNumberOfSaves = true;
    private int _numberOfSavesWhenWarningIsDisplayed = 10;
    private double _sizeOfSaveThumbnails = 115;
    private ListDisplay _listDisplay = ListDisplay.Horizontal;

    private bool _removePortraitsSims = true;
    private bool _removeLotThumbnails;
    private bool _removePhotos;
    private bool _removeTextures;
    private bool _removeGeneratedImages;
    private bool _removeFamilyPortraits = true;
    private bool _removeOtherTypes;
    private bool _createBackup;
    
    private bool _clearCache;
    private string _pathToTheSims3Document = null!;
    private string _pathToSaves = null!;
    private string _pathToBackup = string.Empty;
    private string _pathToFolderWithOldSaves = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OldSaves");
    
    private bool _casPartCacheClear = true;
    private bool _compositorCacheClear = true;
    private bool _scriptCacheClear = true;
    private bool _simCompositorCacheClear = true;
    private bool _socialCacheClear = true;
    private bool _worldCachesClear;
    private bool _igaCacheClear = true;
    private bool _thumbnailsClear = true;
    private bool _featuredItemsClear = true;
    private bool _allXmlClear = true;
    private bool _dccClear = true;
    private bool _downloadedSimsClear = true;
    private bool _missingDepsClear;
    private bool _logClear = true;
    private bool _dcBackupPackagesClear;

    public static readonly RegulSettings Instance;

    static RegulSettings()
    {
        if (!Directory.Exists(PleasantDirectories.Settings))
            Directory.CreateDirectory(PleasantDirectories.Settings);
        
        string regulSettings = Path.Combine(PleasantDirectories.Settings, "RegulSettings.json");

        if (File.Exists(regulSettings))
        {
            try
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(PleasantDirectories.Settings, regulSettings));
                Instance = JsonSerializer.Deserialize(fileStream, RegulSettingsGenerationContext.Default.RegulSettings) ?? throw new NullReferenceException();
            }
            catch
            {
                Instance = new RegulSettings
                {
                    Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                };
        
                Setup();
            }
        }
        else
        {
            Instance = new RegulSettings
            {
                Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
            };
        
            Setup();
        }
    }

    private static void Setup()
    {
#if Windows
        if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
            Instance.FontName = "Microsoft YaHei UI";
        else
            Instance.FontName = FontManager.Current.DefaultFontFamily.Name;
#elif OSX
        if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh")
            Instance.FontName = "PingFang SC";
        else
            Instance.FontName = FontManager.Current.DefaultFontFamily.Name;
#else
        Instance.FontName = FontManager.Current.DefaultFontFamily.Name;
#endif

#if OSX
        Instance.PathToTheSims3Document =
 Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents", "Electronic Arts", LocalizedNames.TheSims3);
        Instance.PathToSaves =
 Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents", "Electronic Arts", LocalizedNames.TheSims3, "Saves");
        Instance.WorldCachesClear = false;
#elif Linux
        Instance.PathToTheSims3Document = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Electronic Arts", LocalizedNames.TheSims3);
        Instance.PathToSaves = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Electronic Arts",
            LocalizedNames.TheSims3, "Saves");
        Instance.WorldCachesClear = true;
        
        PleasantSettings.Instance.WindowSettings.EnableBlur = false;
        PleasantSettings.Instance.WindowSettings.EnableCustomTitleBar = false;
#else
        Instance.PathToTheSims3Document = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Electronic Arts", LocalizedNames.TheSims3);
        Instance.PathToSaves = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Electronic Arts",
            LocalizedNames.TheSims3, "Saves");
        Instance.WorldCachesClear = true;

#if !NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393))
        {
            PleasantSettings.Instance.WindowSettings.EnableBlur = false;
            PleasantSettings.Instance.WindowSettings.EnableCustomTitleBar = false;
        }
#endif
#endif
    }

    public static void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(PleasantDirectories.Settings, "RegulSettings.json"));
        JsonSerializer.Serialize(fileStream, Instance, RegulSettingsGenerationContext.Default.RegulSettings);
    }

    public AvaloniaList<GameSaveResource> GameSaveResources
    {
        get => _gameSaveResource;
        set => RaiseAndSet(ref _gameSaveResource, value);
    }
    
    public string Language
    {
        get => _language;
        set => RaiseAndSet(ref _language, value);
    }

    public string FontName
    {
        get => _fontName;
        set => RaiseAndSet(ref _fontName, value);
    }

    public bool ShowWarningAboutLargeNumberOfSaves
    {
        get => _showWarningAboutLargeNumberOfSaves;
        set => RaiseAndSet(ref _showWarningAboutLargeNumberOfSaves, value);
    }

    public int NumberOfSavesWhenWarningIsDisplayed
    {
        get => _numberOfSavesWhenWarningIsDisplayed;
        set => RaiseAndSet(ref _numberOfSavesWhenWarningIsDisplayed, value);
    }

    public double SizeOfSaveThumbnails
    {
        get => _sizeOfSaveThumbnails;
        set => RaiseAndSet(ref _sizeOfSaveThumbnails, value);
    }

    public ListDisplay ListDisplay
    {
        get => _listDisplay;
        set => RaiseAndSet(ref _listDisplay, value);
    }

    public bool RemovePortraitsSims
    {
        get => _removePortraitsSims;
        set => RaiseAndSet(ref _removePortraitsSims, value);
    }
    
    public bool RemoveLotThumbnails
    {
        get => _removeLotThumbnails;
        set => RaiseAndSet(ref _removeLotThumbnails, value);
    }
    
    public bool RemovePhotos
    {
        get => _removePhotos;
        set => RaiseAndSet(ref _removePhotos, value);
    }
    
    public bool RemoveTextures
    {
        get => _removeTextures;
        set => RaiseAndSet(ref _removeTextures, value);
    }
    
    public bool RemoveGeneratedImages
    {
        get => _removeGeneratedImages;
        set => RaiseAndSet(ref _removeGeneratedImages, value);
    }
    
    public bool RemoveFamilyPortraits
    {
        get => _removeFamilyPortraits;
        set => RaiseAndSet(ref _removeFamilyPortraits, value);
    }
    
    public bool CreateBackup
    {
        get => _createBackup;
        set => RaiseAndSet(ref _createBackup, value);
    }
    
    public bool ClearCache
    {
        get => _clearCache;
        set => RaiseAndSet(ref _clearCache, value);
    }
    
    public bool RemoveOtherTypes
    {
        get => _removeOtherTypes;
        set => RaiseAndSet(ref _removeOtherTypes, value);
    }
    
    public string PathToTheSims3Document
    {
        get => _pathToTheSims3Document;
        set => RaiseAndSet(ref _pathToTheSims3Document, value);
    }
    
    public string PathToSaves
    {
        get => _pathToSaves;
        set => RaiseAndSet(ref _pathToSaves, value);
    }
    
    public string PathToBackup
    {
        get => _pathToBackup;
        set => RaiseAndSet(ref _pathToBackup, value);
    }

    public string PathToFolderWithOldSaves
    {
        get => _pathToFolderWithOldSaves;
        set => RaiseAndSet(ref _pathToFolderWithOldSaves, value);
    }

    // Clear cache
    public bool CasPartCacheClear
    {
        get => _casPartCacheClear;
        set => RaiseAndSet(ref _casPartCacheClear, value);
    }
    
    public bool CompositorCacheClear
    {
        get => _compositorCacheClear;
        set => RaiseAndSet(ref _compositorCacheClear, value);
    }
    
    public bool ScriptCacheClear
    {
        get => _scriptCacheClear;
        set => RaiseAndSet(ref _scriptCacheClear, value);
    }
    
    public bool SimCompositorCacheClear
    {
        get => _simCompositorCacheClear;
        set => RaiseAndSet(ref _simCompositorCacheClear, value);
    }
    
    public bool SocialCacheClear
    {
        get => _socialCacheClear;
        set => RaiseAndSet(ref _socialCacheClear, value);
    }
    
    public bool WorldCachesClear
    {
        get => _worldCachesClear;
        set => RaiseAndSet(ref _worldCachesClear, value);
    }
    
    public bool IgaCacheClear
    {
        get => _igaCacheClear;
        set => RaiseAndSet(ref _igaCacheClear, value);
    }
    
    public bool ThumbnailsClear
    {
        get => _thumbnailsClear;
        set => RaiseAndSet(ref _thumbnailsClear, value);
    }
    
    public bool FeaturedItemsClear
    {
        get => _featuredItemsClear;
        set => RaiseAndSet(ref _featuredItemsClear, value);
    }
    
    public bool AllXmlClear
    {
        get => _allXmlClear;
        set => RaiseAndSet(ref _allXmlClear, value);
    }
    
    public bool DccClear
    {
        get => _dccClear;
        set => RaiseAndSet(ref _dccClear, value);
    }
    
    public bool DownloadedSimsClear
    {
        get => _downloadedSimsClear;
        set => RaiseAndSet(ref _downloadedSimsClear, value);
    }
    
    public bool MissingDepsClear
    {
        get => _missingDepsClear;
        set => RaiseAndSet(ref _missingDepsClear, value);
    }
    
    public bool LogClear
    {
        get => _logClear;
        set => RaiseAndSet(ref _logClear, value);
    }
    
    public bool DcBackupPackagesClear
    {
        get => _dcBackupPackagesClear;
        set => RaiseAndSet(ref _dcBackupPackagesClear, value);
    }

    public static void Reset()
    {
        Setup();
        
        Instance.RemovePortraitsSims = true;
        Instance.RemoveLotThumbnails = false;
        Instance.RemovePhotos = false;
        Instance.RemoveTextures = false;
        Instance.RemoveGeneratedImages = false;
        Instance.RemoveFamilyPortraits = true;
        Instance.RemoveOtherTypes = false;
        Instance.CreateBackup = false;
        
        Instance.ClearCache = false;
        Instance.PathToBackup = string.Empty;
        
        Instance.CasPartCacheClear = true;
        Instance.CompositorCacheClear = true;
        Instance.ScriptCacheClear = true;
        Instance.SimCompositorCacheClear = true;
        Instance.SocialCacheClear = true;
        Instance.IgaCacheClear = true;
        Instance.ThumbnailsClear = true;
        Instance.FeaturedItemsClear = true;
        Instance.AllXmlClear = true;
        Instance.DccClear = true;
        Instance.DownloadedSimsClear = true;
        Instance.MissingDepsClear = false;
        Instance.LogClear = true;
        Instance.DcBackupPackagesClear = false;
    }
}