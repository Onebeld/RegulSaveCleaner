using Avalonia.Controls;

namespace RegulSaveCleaner.Views.InformationPages.ClearCache;

public partial class WorldCachesPage : UserControl
{
    public WorldCachesPage()
    {
        InitializeComponent();
        
#if !OSX
        WarningMacOsDescription.IsVisible = false;
#endif
    }
}