using PleasantUI.Controls;

namespace RegulSaveCleaner.Views.Windows;

public partial class WarningAboutLargeNumberOfSavesWindow : ContentDialog
{
    public WarningAboutLargeNumberOfSavesWindow()
    {
        InitializeComponent();

        CloseButton.Click += (_, _) => Close();
    }
}