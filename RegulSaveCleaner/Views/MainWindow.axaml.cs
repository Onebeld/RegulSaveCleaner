using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.ViewModels;

namespace RegulSaveCleaner.Views;

public partial class MainWindow : PleasantWindow
{
    public MainWindowViewModel ViewModel { get; set; }
    
    public MainWindow() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        Closed += OnClosed;
        
        ViewModel.NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };

#if !NET461
        if (OperatingSystem.IsMacOS())
        {
            EnableTitleBarMargin = true;
            TitleBarType = PleasantTitleBarType.Classic;
        }
#endif

#if DEBUG
        if (Design.IsDesignMode) return;
#endif
        
        ViewModel.LoadingSaves();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        RegulSettings.Save();
        PleasantSettings.Instance.Save();
    }
}