using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PleasantUI;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.ViewModels;
using RegulSaveCleaner.Views;

namespace RegulSaveCleaner;

public class App : Application
{
    public static PleasantTheme PleasantTheme { get; private set; } = null!;

    public static MainWindow MainWindow { get; private set; } = null!;

    public App() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        LoadAllLanguages();
        ChangeLanguage();
        
        PleasantTheme = new PleasantTheme();
        Styles.Add(PleasantTheme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow();
            MainWindow.DataContext = MainWindow.ViewModel = new MainWindowViewModel(MainWindow);
            desktop.MainWindow = MainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    public static void ChangeLanguage()
    {
        Language? language = Languages.FirstOrDefault(x => x.Key == RegulSettings.Instance.Language || x.AdditionalKeys.Any(lang => lang == RegulSettings.Instance.Language));

        string? key = language.Value.Key;

        if (string.IsNullOrWhiteSpace(key))
            key = "en";

        RegulSettings.Instance.Language = key;

        foreach (IResourceProvider resourceProvider in _xamlLanguages)
        {
            if (resourceProvider is not ResourceDictionary resourceDictionary) continue;

            if (resourceDictionary.TryGetResource("LanguageKey", null, out object? value) && value as string == RegulSettings.Instance.Language)
            {
                if (CurrentLanguage is null)
                {
                    CurrentLanguage = resourceDictionary;
                    Current?.Resources.MergedDictionaries.Add(CurrentLanguage);
                }
                else
                {
                    Current?.Resources.MergedDictionaries.Remove(CurrentLanguage);
                    CurrentLanguage = resourceDictionary;
                    Current?.Resources.MergedDictionaries.Add(CurrentLanguage);
                }
            }
        }
    }

    public static string GetString(string key)
    {
        if (Current!.TryFindResource(key, out object? objectText))
            return objectText as string ?? string.Empty;
        return key;
    }

    private static readonly List<IResourceProvider> _xamlLanguages = new();

    private static ResourceDictionary? CurrentLanguage { get; set; }

    /// <summary>
    /// Looks for a suitable resource in the program.
    /// </summary>
    /// <param name="key">Resource name</param>
    /// <typeparam name="T">Resource type</typeparam>
    /// <returns>Resource found, otherwise null</returns>
    public static T GetResource<T>(object key)
    {
        object? value = null;

        IResourceHost? current = Current;

        while (current != null)
        {
            if (current is { } host)
            {
                if (host.TryGetResource(key, out value))
                {
                    return (T)value!;
                }
            }

            current = ((IStyleHost)current).StylingParent as IResourceHost;
        }

        return (T)value!;
    }

    public static readonly Language[] Languages =
    {
        new("Česky (Czech)", "cs"),
        new("Dansk (Danish)", "da"),
        new("Deutsch (German)", "de"),
        new("English (English)", "en"),
        new("Français (French)", "fr"),
        new("Русский (Russian)", "ru"),
        new("日本語 (Japanese)", "ja"),
        new("中文 (Chinese)", "zh"),
    };

    private void LoadAllLanguages()
    {
        IList<IResourceProvider> resourceProviders = GetResource<ResourceDictionary>("LanguageDictionary").MergedDictionaries;

        foreach (IResourceProvider resourceProvider in resourceProviders)
            _xamlLanguages.Add(resourceProvider);
    }
}