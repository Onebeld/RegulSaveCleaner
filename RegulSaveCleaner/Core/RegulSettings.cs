using Avalonia.Collections;
using PleasantUI;
using RegulSaveCleaner.Structures;

namespace RegulSaveCleaner.Core;

public class RegulSettings : ViewModelBase
{
    private AvaloniaList<GameSaveResource> _gameSaveResource = new();
    
    private bool _hardwareAcceleration = true;
    
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

    public static RegulSettings Instance = new();
    
    public AvaloniaList<GameSaveResource> GameSaveResources
    {
        get => _gameSaveResource;
        set => RaiseAndSet(ref _gameSaveResource, value);
    }
    
    public bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set => RaiseAndSet(ref _hardwareAcceleration, value);
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
}