﻿using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using PleasantUI;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Windows;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Core.Constants;
using RegulSaveCleaner.Core.Extensions;
using RegulSaveCleaner.Structures;
using Color = Avalonia.Media.Color;

namespace RegulSaveCleaner.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private bool _isClearingOldProhibitedLists;
    
    public AvaloniaList<FontFamily> Fonts { get; } = new();

    public FontFamily SelectedFontFamily
    {
        get => FontFamily.Parse(RegulSettings.Instance.FontName);
        set => RegulSettings.Instance.FontName = value.Name;
    }

    public Language SelectedLanguage
    {
        get => App.Languages.First(language => language.Key == RegulSettings.Instance.Language);
        set
        {
            RegulSettings.Instance.Language = value.Key;
            App.ChangeLanguage();

            App.MainWindow.SettingsPage.IsSelected = false;
            App.MainWindow.SettingsPage.IsSelected = true;
        }
    }

    public int SelectedIndexTheme
    {
        get
        {
            return PleasantSettings.Instance.Theme switch
            {
                Theme.Light => 1,
                Theme.Dark => 2,
                
                _ => 0
            };
        }
        set
        {
            PleasantSettings.Instance.Theme = value switch
            {
                1 => Theme.Light,
                2 => Theme.Dark,

                _ => Theme.System
            };
            
            App.PleasantTheme.UpdateTheme();
        }
    }

    public int SelectedIndexListDisplay
    {
        get
        {
            return RegulSettings.Instance.ListDisplay switch
            {
                ListDisplay.Vertical => 1,
                _ => 0
            };
        }
        set
        {
            RegulSettings.Instance.ListDisplay = value switch
            {
                1 => ListDisplay.Vertical,
                _ => ListDisplay.Horizontal
            };
        }
    }

    public int SelectedIndexSaveFilesVerticalListDisplay
    {
        get
        {
            return RegulSettings.Instance.SaveFilesVerticalListDisplay switch
            {
                SaveFilesVerticalListDisplay.Block => 1,
                _ => 0
            };
        }
        set
        {
            RegulSettings.Instance.SaveFilesVerticalListDisplay = value switch
            {
                1 => SaveFilesVerticalListDisplay.Block,
                _ => SaveFilesVerticalListDisplay.Line
            };
        }
    }

    public bool UseAccentColor
    {
        get => !PleasantSettings.Instance.PreferUserAccentColor;
        set
        {
            PleasantSettings.Instance.PreferUserAccentColor = !value;

            if (PleasantSettings.Instance.PreferUserAccentColor)
                return;

            Color accent = Application.Current!.PlatformSettings!.GetColorValues().AccentColor1;

            PleasantSettings.Instance.NumericalAccentColor = accent.ToUInt32();
            App.PleasantTheme.UpdateAccentColors(accent);
        }
    }

    public bool IsClearingOldProhibitedLists
    {
        get => _isClearingOldProhibitedLists;
        set => RaiseAndSet(ref _isClearingOldProhibitedLists, value);
    }

    public bool IsLinux
    {
        get
        {
#if Linux
            return true;
#else
            return false;
#endif
        }
    }
    
    public bool IsWindows7
    {
        get
        {
#if Windows
            
#if !NET6_0_OR_GREATER
            return Environment.OSVersion.Version > new Version(10, 0, 14393);

#else
            return false;
#endif

#else
            return false;
#endif
        }
    }

    public SettingsViewModel()
    {
        Fonts.AddRange(FontManager.Current.SystemFonts.OrderBy(x => x.Name));
    }

    public async void ChoosePathForOldSaves()
    {
        string? path = await StorageProvider.SelectDirectory(App.MainWindow);

        if (!string.IsNullOrWhiteSpace(path))
            RegulSettings.Instance.PathToFolderWithOldSaves = path;
    }
    
    public async void ChoosePathToTheSims3Documents()
    {
        string? path = await StorageProvider.SelectDirectory(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        if (!string.IsNullOrWhiteSpace(path))
            RegulSettings.Instance.PathToTheSims3Document = path;

        await App.MainWindow.ViewModel.LoadSaves();
    }
    
    public async void ChoosePathToSave()
    {
        string? path = await StorageProvider.SelectDirectory(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        if (!string.IsNullOrWhiteSpace(path))
            RegulSettings.Instance.PathToSaves = path;
        
        await App.MainWindow.ViewModel.LoadSaves();
    }
    
    public async void ClearOldProhibitedLists()
    {
        bool clean = false;
        
        IsClearingOldProhibitedLists = true;
        await Task.Run(() =>
        {
            foreach (GameSaveResource resource in RegulSettings.Instance.GameSaveResources)
            {
                bool delete = App.MainWindow.ViewModel.GameSaves.All(saveFile => saveFile.Name != resource.Id);

                if (delete)
                {
                    RegulSettings.Instance.GameSaveResources.Remove(resource);
                    clean = true;
                }
            }
        });
        IsClearingOldProhibitedLists = false;

        if (clean)
        {
            App.MainWindow.ViewModel.NotificationManager.Show(new Notification(App.GetString("Successful"),
                App.GetString("UnusedProhibitedListsCleared"),
                NotificationType.Success,
                TimeSpan.FromSeconds(3)));
        }
        else
        {
            App.MainWindow.ViewModel.NotificationManager.Show(new Notification(App.GetString("Information"),
                App.GetString("UnusedProhibitedListsNotCleared"),
                NotificationType.Information,
                TimeSpan.FromSeconds(3)));
        }
    }
    
    public async void ChangeAccentColor()
    {
        Color? newColor = await ColorPickerWindow.SelectColor(App.MainWindow, PleasantSettings.Instance.NumericalAccentColor);

        if (newColor is not { } color)
            return;

        PleasantSettings.Instance.NumericalAccentColor = color.ToUInt32();
        App.PleasantTheme.UpdateAccentColors(color);
    }
    
    public async void PasteAccentColor()
    {
        string? data = await App.MainWindow.Clipboard?.GetTextAsync()!;

        if (uint.TryParse(data, out uint uintColor))
        {
            PleasantSettings.Instance.NumericalAccentColor = uintColor;
            App.PleasantTheme.UpdateAccentColors(Color.FromUInt32(uintColor));
        }
        else if (Color.TryParse(data, out Color color))
        {
            PleasantSettings.Instance.NumericalAccentColor = color.ToUInt32();
            App.PleasantTheme.UpdateAccentColors(color);
        }
    }
    
    public async void CopyAccentColor()
    {
        await App.MainWindow.Clipboard?.SetTextAsync(
            $"#{PleasantSettings.Instance.NumericalAccentColor.ToString("x8").ToUpper()}")!;
        
        App.MainWindow.ViewModel.NotificationManager.Show(new Notification(App.GetString("Information"),
            App.GetString("ColorCopied"),
            NotificationType.Information,
            TimeSpan.FromSeconds(2)));
    }
    
    public async void ResetSettings()
    {
        string result = await MessageBox.Show(App.MainWindow, App.GetString("ResetSettingsWarning"), string.Empty, MessageBoxButtons.ReverseYesNo);

        if (result != "Yes") return;
        
        SelectedIndexTheme = 0;
        
        RegulSettings.Reset();
        PleasantSettings.Instance.Reset();

        App.ChangeLanguage();
        
        RaisePropertyChanged(nameof(SelectedLanguage));
        RaisePropertyChanged(nameof(SelectedFontFamily));
        RaisePropertyChanged(nameof(SelectedIndexTheme));

        App.MainWindow.ViewModel.NotificationManager.Show(new Notification(App.GetString("Successful"),
            App.GetString("SettingsHaveBeenReset"),
            NotificationType.Success,
            TimeSpan.FromSeconds(3)));
    }
}