using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.ViewModels;
using RegulSaveCleaner.Views.Pages;

namespace RegulSaveCleaner.Views;

public partial class MainWindow : PleasantWindow
{
    public MainWindowViewModel ViewModel { get; set; } = null!;
    
    public MainWindow()
    {
        InitializeComponent();

        CleanerPage.FuncControl += () => new CleanerPage();
        QuestionsAndAnswersPage.FuncControl += () => new QuestionsAndAnswersPage();
        SettingsPage.FuncControl += () => new SettingsPage();
        AboutPage.FuncControl += () => new AboutPage();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        Closed += OnClosed;
        Closing += OnClosing;
        
        ViewModel.NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
        
#if OSX
        EnableTitleBarMargin = true;
        TitleBarType = PleasantTitleBarType.Classic;
        TitleBarStyle = PleasantTitleBarStyle.MacOS;
#endif
        
#if DEBUG
        if (Design.IsDesignMode) return;
#endif
        
        ViewModel.LoadingSaves();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (ViewModel.InCleaningProcess || ViewModel.InCleaningCacheProcess || ViewModel.InCreatingBackupProcess)
            e.Cancel = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        RegulSettings.Save();
        PleasantSettings.Instance.Save();
    }
}