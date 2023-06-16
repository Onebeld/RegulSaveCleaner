using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PleasantUI;
using RegulSaveCleaner.ViewModels;
using RegulSaveCleaner.Views;

namespace RegulSaveCleaner;

public partial class App : Application
{
    public PleasantTheme PleasantTheme { get; private set; }

    public static MainWindow MainWindow { get; private set; } = null!;

    public App() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        PleasantTheme = new PleasantTheme();
        Styles.Add(PleasantTheme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.MainWindow = MainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
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
}