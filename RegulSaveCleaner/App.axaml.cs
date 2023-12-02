using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.ViewModels;
using RegulSaveCleaner.Views;

namespace RegulSaveCleaner;

public class App : Application
{
    private static readonly List<IResourceProvider> _xamlLanguages = new();
    
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

    /// <summary>
    /// Changes the application's user interface language.
    /// </summary>
    /// <remarks>
    /// The <see cref="ChangeLanguage"/> method changes the application's language based on the value in <see cref="RegulSettings.Instance.Language"/>,
    /// which holds the language key. The language keys are searched within the <see cref="Languages"/> array and each language's additional keys.
    /// If the key is empty or null, it defaults to "en" (English). After setting the language key, it attempts to update each resource dictionary
    /// within the collection of <see cref="_xamlLanguages"/>. If the language key exists in the dictionary and matches with <see cref="RegulSettings.Instance.Language"/>,
    /// the changes are applied and the current language dictionary is updated.
    /// </remarks>
    public static void ChangeLanguage()
    {
        Language? language = Array.Find(Languages, x => x.Key == RegulSettings.Instance.Language || Array.Exists(x.AdditionalKeys, lang => lang == RegulSettings.Instance.Language));
        
        string? key = language.Value.Key;

        if (string.IsNullOrWhiteSpace(key))
            key = "en";

        RegulSettings.Instance.Language = key;

        foreach (IResourceProvider resourceProvider in _xamlLanguages)
        {
            if (resourceProvider is not ResourceDictionary resourceDictionary) continue;

            if (!resourceDictionary.TryGetResource("LanguageKey", null, out object? value) || value as string != RegulSettings.Instance.Language)
                continue;

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
    
    /// <summary>
    /// Retrieves a string localized resource by a specific key.
    /// </summary>
    /// <param name="key">Key associated with the string localized resource to retrieve.</param>.
    /// <returns>
    /// Returns the string localized resource associated with the specified key if found, otherwise returns the key itself.
    /// If the localized resource is found but it is not a string, returns an empty string.
    /// </returns>
    public static string GetString(string key)
    {
        if (Current!.TryFindResource(key, out object? localizedObject))
            return localizedObject as string ?? string.Empty;
        return key;
    }

    private static ResourceDictionary? CurrentLanguage { get; set; }

    public static readonly Language[] Languages =
    {
        new("Čeština (Czech)", "cs"),
        new("Dansk (Danish)", "da"),
        new("Deutsche (German)", "de"),
        new("English (English)", "en"),
        new("Español (Spanish)", "es"),
        new("Français (French)", "fr"),
        new("Italiano (Italian)", "it"),
        new("Nederlands (Dutch)", "nl"),
        new("Norsk (Norwegian)", "nb", "no", "nn"),
        new("Polskie (Polish)", "pl"),
        new("Português (Portuguese)", "pt"),
        new("Svenska (Swedish)", "sv"),
        new("Suomalainen (Finnish)", "fi"),
        new("Русский (Russian)", "ru"),
        new("日本語 (Japanese)", "ja"),
        new("中文 (Chinese)", "zh"),
        new("한국어 (Korean)", "ko")
    };

    private static void LoadAllLanguages()
    {
        IList<IResourceProvider> resourceProviders =  ((ResourceDictionary)Current!.FindResource("LanguageDictionary")!).MergedDictionaries;

        foreach (IResourceProvider resourceProvider in resourceProviders)
            _xamlLanguages.Add(resourceProvider);
    }
}