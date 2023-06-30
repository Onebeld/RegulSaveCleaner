using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RegulSaveCleaner.Controls.SaveFilesList;

public partial class VerticalSaveFilesList : UserControl
{
    public VerticalSaveFilesList()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}