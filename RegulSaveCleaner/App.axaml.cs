using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using PleasantUI;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.ViewModels;
using RegulSaveCleaner.Views;

namespace RegulSaveCleaner;

public partial class App : Application
{
    public PleasantTheme PleasantTheme { get; private set; }

    public static MainWindow MainWindow { get; private set; } = null!;

    public App() => AvaloniaXamlLoader.Load(this);
    
    public static ResourceInclude LanguageResourceInclude { get; private set; } = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeLanguage();
        
        PleasantTheme = new PleasantTheme();
        Styles.Add(PleasantTheme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow();
            MainWindow.DataContext = new MainWindowViewModel(MainWindow);
            desktop.MainWindow = MainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeLanguage()
    {
        Language? language = Languages.FirstOrDefault(x => x.Key == RegulSettings.Instance.Language 
                                                           || x.AdditionalKeys.Any(lang => lang == RegulSettings.Instance.Language));

        string? key = language.Value.Key;

        if (string.IsNullOrWhiteSpace(key))
            key = "en";

        RegulSettings.Instance.Language = key;

        LanguageResourceInclude = new ResourceInclude((Uri?)null)
        {
            Source = new Uri($"avares://RegulSaveCleaner.Assets/Localization/{RegulSettings.Instance.Language}.axaml")
        };
        Resources.MergedDictionaries.Add(LanguageResourceInclude);
    }

    public static void ChangeLanguage(string key)
    {
        LanguageResourceInclude = new ResourceInclude((Uri?)null)
        {
            Source = new Uri($"avares://RegulSaveCleaner.Assets/Localization/{key}.axaml")
        };
    }
    
    public static string GetString(string key)
    {
        if (Current!.TryFindResource(key, out object? objectText))
            return objectText as string ?? string.Empty;
        return key;
    }
    
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
        new("English (English)", "en")
    };
}