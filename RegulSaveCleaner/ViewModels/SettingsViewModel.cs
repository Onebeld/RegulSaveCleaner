using Avalonia.Collections;
using Avalonia.Media;
using PleasantUI;
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