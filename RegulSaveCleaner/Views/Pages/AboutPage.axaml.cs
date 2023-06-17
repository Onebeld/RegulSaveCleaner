using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace RegulSaveCleaner.Views.Pages;

public partial class AboutPage : UserControl
{
    public AboutPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        RunDotNet.Text = $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}";
        RunAuthor.Text = $"©2020-{DateTime.Now.Year} Dmitry Zhutkov (Onebeld)";
        
        GitHubButton.Click += GitHubButtonOnClick;
        PatreonButton.Click += PatreonButtonOnClick;
    }

    private void PatreonButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/onebeld",
            UseShellExecute = true
        });
    }

    private void GitHubButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Onebeld/RegulSaveCleaner",
            UseShellExecute = true
        });
    }
}