using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using PleasantUI.Windows;
using RegulSaveCleaner.Core.Constants;

namespace RegulSaveCleaner.Controls;

public partial class ClearedCacheNotification : UserControl, INotification
{
    public ClearedCacheNotification()
    {
        InitializeComponent();
        Button.Click += ButtonOnClick;
    }

    private void ButtonOnClick(object? sender, RoutedEventArgs e)
    {
        StringBuilder stringBuilder = new();
        
        foreach (string deletedFile in DeletedFiles)
            stringBuilder.Append($"{deletedFile}\n");
        
        MessageBox.Show(App.MainWindow, App.GetString("CacheCleared"), App.GetString("ResultsBelow"), MessageBoxButtons.Ok, stringBuilder.ToString());
        
        ((NotificationCard)Parent).Close();
    }

    public string? Title { get; }

    public string? Message { get; }

    public NotificationType Type { get; set; }

    public TimeSpan Expiration { get; }

    public Action? OnClick { get; }

    public Action? OnClose { get; }
    
    public List<string> DeletedFiles { get; set; }
}