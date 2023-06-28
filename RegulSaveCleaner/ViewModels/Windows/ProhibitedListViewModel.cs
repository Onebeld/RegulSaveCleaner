using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Pfim;
using PleasantUI;
using PleasantUI.Extensions;
using PleasantUI.Reactive;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Core.Extensions;
using RegulSaveCleaner.S3PI.Package;
using RegulSaveCleaner.S3PI.Resources;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.Views.Windows;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace RegulSaveCleaner.ViewModels.Windows;

public class ProhibitedListViewModel : ViewModelBase
{
    private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current!;
    
    private static readonly object Obj = new();
    
    private bool _isLoaded;
    private bool _isCompleting;
    private int _selectedTypeIndex;

    private readonly string _saveId;
    private readonly GameSave _gameSave;
    
    private AvaloniaList<ImageResource> _selectedFundedImageResources = new();
    private AvaloniaList<ImageResource> _fundedImageResources = new();

    public bool IsLoaded
    {
        get => _isLoaded;
        set => RaiseAndSet(ref _isLoaded, value);
    }

    public bool IsCompleting
    {
        get => _isCompleting;
        set => RaiseAndSet(ref _isCompleting, value);
    }

    public int SelectedTypeIndex
    {
        get => _selectedTypeIndex;
        set => RaiseAndSet(ref _selectedTypeIndex, value);
    }

    public AvaloniaList<ImageResource> SelectedFundedImageResources
    {
        get => _selectedFundedImageResources;
        set => RaiseAndSet(ref _selectedFundedImageResources, value);
    }

    public AvaloniaList<ImageResource> SelectedImageResources { get; } = new();

    public AvaloniaList<ImageResource> FundedImageResources
    {
        get => _fundedImageResources;
        set => RaiseAndSet(ref _fundedImageResources, value);
    }

    public AvaloniaList<ImageResource> ImageResources { get; } = new();

    public ProhibitedListViewModel(GameSave gameSave)
    {
        ImageResources.CollectionChanged += ImageResourcesOnCollectionChanged;
        SelectedFundedImageResources.CollectionChanged += SelectedFundedImageResourcesOnCollectionChanged;
        
        _gameSave = gameSave;
        _saveId = gameSave.Name;

        this.WhenAnyValue(x => x.SelectedTypeIndex)
            .Skip(1)
            .Subscribe(OnChangeSelectedTypeIndex);
    }

    private void SelectedFundedImageResourcesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsLoaded) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                SelectedImageResources.Add((ImageResource)e.NewItems?[0]!);
                break;
            case NotifyCollectionChangedAction.Remove:
                SelectedImageResources.Remove((ImageResource)e.OldItems?[0]!);
                break;
        }
    }

    private void OnChangeSelectedTypeIndex(int index)
    {
        IsLoaded = true;
        FundedImageResources.Clear();
        switch (index)
        {
            // Photos
            case 1:
                foreach (ImageResource imageResource in ImageResources)
                {
                    if (CheckResourceAvailability(GameDataTypes.Photos, imageResource))
                        FundedImageResources.Add(imageResource);
                }
                break;
            // Generated Images
            case 2:
                foreach (ImageResource imageResource in ImageResources)
                {
                    if (CheckResourceAvailability(GameDataTypes.GeneratedImages, imageResource))
                        FundedImageResources.Add(imageResource);
                }
                break;
            // Portraits of Sims
            case 3:
                foreach (ImageResource imageResource in ImageResources)
                {
                    if (CheckResourceAvailability(GameDataTypes.SimPortraits, imageResource))
                        FundedImageResources.Add(imageResource);
                }
                break;
            // Portraits of Families
            case 4:
                foreach (ImageResource imageResource in ImageResources)
                {
                    if (CheckResourceAvailability(GameDataTypes.FamilyPortraits, imageResource))
                        FundedImageResources.Add(imageResource);
                }
                break;
            // Lot thumbnails
            case 5:
                foreach (ImageResource imageResource in ImageResources)
                {
                    if (CheckResourceAvailability(GameDataTypes.LotThumbnails, imageResource))
                        FundedImageResources.Add(imageResource);
                }
                break;
            
            // All
            default:
                FundedImageResources = new AvaloniaList<ImageResource>(ImageResources);
                break;
        }

        SelectedFundedImageResources.Clear();
        SelectedFundedImageResources.AddRange(SelectedImageResources);
        IsLoaded = false;
    }

    private bool CheckResourceAvailability(GameDataType gameDataType, ImageResource imageResource)
    {
        bool isAvailable = gameDataType.ResourceTypes.Any(x => x == imageResource.Type);

        if (gameDataType.ResourceGroups is not null)
            isAvailable = gameDataType.ResourceGroups.Any(x => x == imageResource.Group);
        
        return isAvailable;
    }

    private void ImageResourcesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add) 
            FundedImageResources.Add((ImageResource)e.NewItems![0]!);
    }

    public async void LoadImages()
    {
        IsLoaded = true;
        await Task.Run(() =>
        {
            GameSaveResource? gameSaveResource = RegulSettings.Instance.GameSaveResources.FirstOrDefault(x => x.Id == _saveId);
            
            foreach (string file in Directory.EnumerateFiles(_gameSave.Directory, "*.nhd"))
            {
                Package package = Package.OpenPackage(file);

                Parallel.ForEach(package.GetResourceList, resource =>
                {
                    Bitmap bitmap;

                    switch (resource.ResourceType)
                    {
                        // SNAPs
                        case 0x6B6D837E:
                        case 0x6B6D837D:
                        case 0x0580A2CD:
                        case 0xD84E7FC6:
                        case 0x0580A2CE:
                        case 0x0580A2CF:
                        case 0x6B6D837F:
                            DefaultResource resource2;
                            lock (Obj)
                                resource2 = S3PI.WrapperDealer.GetResource(package, resource);

                            bitmap = new Bitmap(resource2.Stream);

                            break;

                        //Generated images
                        case 0x00B2D882
                            when resource.ResourceGroup is 0x0E9928A8 or 0x24B9FCA or 0x2722299 or 0x2BD69A0:
                        case 0x00B2D882 when resource.ResourceGroup == 0x269D005:
                            DefaultResource resource1;
                            lock (Obj)
                                resource1 = S3PI.WrapperDealer.GetResource(package, resource);

                            IImage image =
                                Dds.Create(resource1.Stream, new PfimConfig());

                            byte[] data = GetData<Bgra32>(new PngEncoder
                            {
                                ColorType = PngColorType.RgbWithAlpha
                            }, image);

                            using (MemoryStream stream = new(data))
                                bitmap = new Bitmap(stream);

                            break;

                        // Other
                        default:
                            return;
                    }

                    ImageResource imageResource = new()
                    {
                        Instance = resource.Instance,
                        Type = resource.ResourceType,
                        Group = resource.ResourceGroup,
                        Image = bitmap,
                        CompressedImage = bitmap.Compress()
                    };

                    _synchronizationContext.Send(_ => { ImageResources.Add(imageResource); }, "");

                    if (gameSaveResource is not null)
                    {
                        foreach (ProhibitedResource prohibitedResource in gameSaveResource.ProhibitedResources)
                        {
                            if (imageResource.Type == prohibitedResource.Type &&
                                imageResource.Group == prohibitedResource.Group &&
                                imageResource.Instance == prohibitedResource.Instance)
                            {
                                _synchronizationContext.Send(_ =>
                                {
                                    SelectedImageResources.Add(imageResource);
                                    SelectedFundedImageResources.Add(imageResource);
                                }, "");
                            }
                        }
                    }
                });

                Package.ClosePackage(package);
            }
        });
        IsLoaded = false;
    }

    public async void Close(ProhibitedListWindow window)
    {
        IsCompleting = true;
        IsLoaded = true;
        await Task.Run(() =>
        {
            GameSaveResource? saveResources = RegulSettings.Instance.GameSaveResources.FirstOrDefault(x => x.Id == _saveId);

            if (SelectedImageResources.Count == 0 && saveResources is not null)
            {
                RegulSettings.Instance.GameSaveResources.Remove(saveResources);
            }
            else
            {
                if (saveResources == null)
                {
                    saveResources = new GameSaveResource
                    {
                        Id = _saveId
                    };
            
                    RegulSettings.Instance.GameSaveResources.Add(saveResources);
                }

                saveResources.ProhibitedResources = new AvaloniaList<ProhibitedResource>();

                foreach (ImageResource imageResource in SelectedImageResources)
                {
                    saveResources.ProhibitedResources.Add(new ProhibitedResource
                    {
                        Type = imageResource.Type,
                        Group = imageResource.Group,
                        Instance = imageResource.Instance
                    });
                }
            }
        });

        window.Close();
    }

    public void CloseWithoutSave(ProhibitedListWindow window) => window.Close();

    public async void SaveSelectedImages()
    {
        string? path = await StorageProvider.SelectFolder(App.MainWindow);

        if (!string.IsNullOrWhiteSpace(path))
        {
            IsLoaded = true;
            IsCompleting = true;
            await Task.Run(() =>
            {
                foreach (ImageResource imageResource in SelectedImageResources)
                {
                    string filePath = Path.Combine(path, $"{imageResource.Type}-{imageResource.Group}-{imageResource.Instance}.png");
                    
                    imageResource.Image?.Save(filePath);
                }
            });
            IsLoaded = false;
            IsCompleting = false;
            
            App.MainWindow.ViewModel.NotificationManager.Show(new Notification(App.GetString("Successful"),
                App.GetString("ImagesSaved"),
                NotificationType.Success,
                TimeSpan.FromSeconds(3)));
        }
    }

    public void UnselectResources()
    {
        SelectedImageResources.Clear();
        SelectedFundedImageResources.Clear();
    }

    private static byte[] GetData<T>(IImageEncoder encoder, IImage dds) where T : unmanaged, IPixel<T>
    {
        Image<T> image = Image.LoadPixelData<T>(dds.Data, dds.Width, dds.Height);
        using MemoryStream stream = new();
        image.Save(stream, encoder);
        return stream.ToArray();
    }
}