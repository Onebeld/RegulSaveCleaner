using PleasantUI.Controls;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.ViewModels.Windows;

namespace RegulSaveCleaner.Views.Windows;

public partial class OldProhibitedListsWindow : ContentDialog
{
    public OldProhibitedListsWindow() => InitializeComponent();
    
    public OldProhibitedListsWindow(GameSave saveFile, IEnumerable<string> saveNames) : this()
    {
        DataContext = new OldProhibitedListsViewModel(saveFile, saveNames);
    }
}