using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace RegulSaveCleaner.Views.Pages;

public partial class QuestionsAndAnswersPage : UserControl
{
    public QuestionsAndAnswersPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        InstallationButton.Click += InstallationButtonOnClick;
        OptimizationManualButton.Click += OptimizationManualButtonOnClick;
        Error12ManualButton.Click += Error12ManualButtonOnClick;
    }

    private void Error12ManualButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=264738834",
            UseShellExecute = true
        });
    }

    private void OptimizationManualButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://steamcommunity.com/sharedfiles/filedetails/?id=1131162350",
            UseShellExecute = true
        });
    }

    private void InstallationButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://modthesims.info/d/658759",
            UseShellExecute = true
        });
    }
}