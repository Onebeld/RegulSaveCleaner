using Avalonia.Collections;
using Avalonia.Media;
using PleasantUI;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;

namespace RegulSaveCleaner.ViewModels;

public class SettingsViewModel : ViewModelBase
{
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
            App.ChangeLanguage(value.Key);
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
            
            App.PleasantTheme.UpdateAccentColors(Color.FromUInt32(PleasantSettings.Instance.NumericalAccentColor));
        }
    }

    public SettingsViewModel()
    {
        Fonts.AddRange(FontManager.Current.SystemFonts.OrderBy(x => x.Name));
    }
    
    public async void ChoosePath()
    {
        string? path = await StorageProvider.SelectFolder(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        if (!string.IsNullOrWhiteSpace(path))
            RegulSettings.Instance.PathToTheSims3Document = path;
    }
    
    public async void ChoosePathToSave()
    {
        string? path = await StorageProvider.SelectFolder(App.MainWindow, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        if (!string.IsNullOrWhiteSpace(path))
            RegulSettings.Instance.PathToSaves = path;
    }
}